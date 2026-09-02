using System.Security.Cryptography;
using System.Text;
using Engine.Serialization.Binary.Checksum;
using Engine.Serialization.Binary.Compression;
using Engine.Serialization.Binary.Encryption;
using Engine.Serialization.Binary.Exceptions;
using Engine.Serialization.Binary.Metadata;

namespace Engine.Serialization.Binary.Codec;

internal sealed class V1FormatCodec(
    ICompressor compressor,
    IChecksumCalculator checksum,
    IEncryptor encryptor) : IFormatCodec
{
    public int Version => 1;

    public void Serialize<T>(Stream destination, T data) where T : class
    {
        ArgumentNullException.ThrowIfNull(destination);

        byte[] rawPayload = SerializePayload(data);
        byte[] checksumBytes = checksum.Compute(rawPayload);
        byte[] compressedPayload = compressor.Compress(rawPayload);
        byte[] onDiskPayload = encryptor.Encrypt(compressedPayload);

        var header = new BinaryFormatHeaderV1(
            Version,
            compressor.DefaultKind, compressor.DefaultCustomName,
            checksum.DefaultKind, checksum.DefaultCustomName,
            encryptor.DefaultKind, encryptor.DefaultCustomName,
            encryptor.DefaultKeyId,
            rawPayload.Length, compressedPayload.Length, onDiskPayload.Length,
            checksumBytes);

        using var writer = new BinaryWriter(destination, Encoding.UTF8, leaveOpen: true);
        header.WriteTo(writer);
        writer.Write(onDiskPayload);
        writer.Flush();
    }

    public T? Deserialize<T>(Stream source) where T : class
    {
        ArgumentNullException.ThrowIfNull(source);

        using var reader = new BinaryReader(source, Encoding.UTF8, leaveOpen: true);
        var header = BinaryFormatHeaderV1.ReadFrom(reader);

        if (header.FormatVersion != Version)
            throw new BinaryFormatNotSupportedException($"Binary format version {header.FormatVersion} is not supported by the V1 codec.");

        byte[] onDiskPayload = reader.ReadBytes(header.OnDiskLength);
        if (onDiskPayload.Length != header.OnDiskLength)
            throw new BinaryFormatException($"Payload ended early. Expected {header.OnDiskLength} bytes, got {onDiskPayload.Length}.");
        
        if (header.KeyId is not null && encryptor.DefaultKeyId is not null && header.KeyId != encryptor.DefaultKeyId)
            throw new BinaryIntegrityException(
                $"This data is marked as encrypted with key '{header.KeyId}', but the configured encryptor is set up for key '{encryptor.DefaultKeyId}'.");

        byte[] compressedPayload;
        try
        {
            compressedPayload = encryptor.Decrypt(header.Encryption, header.CustomEncryptionName, onDiskPayload, header.CompressedLength);
        }
        catch (CryptographicException ex)
        {
            throw new BinaryIntegrityException("Decryption failed: wrong key or tampered payload.", ex);
        }

        byte[] rawPayload = compressor.Decompress(header.Compression, header.CustomCompressionName, compressedPayload, header.UncompressedLength);

        checksum.Verify(header.ChecksumAlgorithm, header.CustomChecksumName, rawPayload, header.Checksum);

        return DeserializePayload<T>(rawPayload);
    }

    private static byte[] SerializePayload<T>(T data) where T : class
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms, Encoding.UTF8, leaveOpen: true);

        new BinaryPayloadWriter(writer).Serialize(data);
        writer.Flush();
        return ms.ToArray();
    }

    private static T? DeserializePayload<T>(byte[] rawPayload) where T : class
    {
        using var ms = new MemoryStream(rawPayload);
        using var reader = new BinaryReader(ms, Encoding.UTF8, leaveOpen: true);

        return new BinaryPayloadReader(reader).Deserialize<T>();
    }
}