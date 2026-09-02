using Engine.Serialization.Binary.Codec;
using Engine.Serialization.Binary.Format;
using Engine.Serialization.Binary.Versioning;
using Engine.Serialization.Binary.Checksum;
using Engine.Serialization.Binary.Compression;
using Engine.Serialization.Binary.Encryption;

namespace Engine.Serialization.Binary;

public sealed class BinarySerializer
{
    private readonly BinaryFormatRouter _router;
    private readonly int _writeVersion;
    
    public BinarySerializer(
        ICompressor? compressor = null,
        IChecksumCalculator? checksum = null,
        IEncryptor? encryptor = null,
        int? writeVersion = null)
    {
        var v1 = new V1FormatCodec(
            compressor ?? new Compressor(new NoCompression()),
            checksum ?? new ChecksumCalculator(new Crc32()),
            encryptor ?? Encryptor.None());

        var v0 = new V0FormatCodec();

        _router = new BinaryFormatRouter([v0, v1]);
        _writeVersion = writeVersion ?? BinaryFormatConstants.LatestVersion;
    }

    public byte[] Serialize<T>(T data) where T : class
    {
        using var ms = new MemoryStream();
        _router.Serialize(ms, data, _writeVersion);
        return ms.ToArray();
    }

    public T? Deserialize<T>(byte[] bytes) where T : class
    {
        using var ms = new MemoryStream(bytes);
        return _router.Deserialize<T>(ms);
    }
}