using System.Text;
using Engine.Serialization.Binary.Checksum;
using Engine.Serialization.Binary.Compression;
using Engine.Serialization.Binary.Encryption;
using Engine.Serialization.Binary.Exceptions;
using Engine.Serialization.Binary.Format;
using Engine.Serialization.Binary.Metadata;

namespace Engine.Serialization.Binary.Codec;

internal sealed class V1FormatCodec(
    ICompressor compressor,
    IChecksumCalculator checksum,
    IEncryptor encryptor) : IFormatCodec
{
    private readonly ICompressor _compressor = compressor;
    private readonly IChecksumCalculator _checksum = checksum;
    private readonly IEncryptor _encryptor = encryptor;

    public int Version => 1;

    public void Serialize<T>(Stream destination, T data) where T : class
    {
        ArgumentNullException.ThrowIfNull(destination);

        byte[] rawPayload = SerializePayload(data);
        byte[] checksumBytes = _checksum.Compute(rawPayload);
        byte[] compressedPayload = _compressor.Compress(rawPayload);
        byte[] onDiskPayload = _encryptor.Encrypt(compressedPayload);

        string? keyId = _encryptor.DefaultKind == EncryptionAlgorithm.None
            ? null
            : _encryptor.DefaultKeyId;

        var header = new BinaryFormatHeaderV1(
            FormatVersion: Version,
            Compression: _compressor.DefaultKind,
            ChecksumAlgorithm: _checksum.DefaultKind,
            Encryption: _encryptor.DefaultKind,
            KeyId: keyId,
            UncompressedLength: rawPayload.Length,
            CompressedLength: compressedPayload.Length,
            OnDiskLength: onDiskPayload.Length,
            Checksum: checksumBytes);

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
            throw new BinaryFormatValidationException(
                $"Binary format version {header.FormatVersion} is not supported by V1 codec.");

        byte[] onDiskPayload = reader.ReadBytes(header.OnDiskLength);
        if (onDiskPayload.Length != header.OnDiskLength)
            throw new BinaryFormatValidationException(
                $"Payload ended early. Expected {header.OnDiskLength} bytes, got {onDiskPayload.Length}.");

        byte[] compressedPayload = _encryptor.Decrypt(
            header.Encryption,
            header.KeyId,
            onDiskPayload,
            header.CompressedLength);

        byte[] rawPayload = _compressor.Decompress(
            header.Compression,
            compressedPayload,
            header.UncompressedLength);

        _checksum.Verify(header.ChecksumAlgorithm, rawPayload, header.Checksum);

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