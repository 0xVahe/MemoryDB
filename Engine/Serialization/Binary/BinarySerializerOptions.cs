using Engine.Serialization.Binary.Format;
using Engine.Serialization.Binary.Checksum;
using Engine.Serialization.Binary.Compression;
using Engine.Serialization.Binary.Encryption;

namespace Engine.Serialization.Binary;

public sealed record BinarySerializerOptions
{
    public static BinarySerializerOptions Default { get; } = new();

    public ICompressor Compressor { get; init; } = Compression.Compressor.None();
    public IChecksumCalculator Checksum { get; init; } = ChecksumCalculator.None();
    public IEncryptor Encryptor { get; init; } = Encryption.Encryptor.None();
    public int WriteVersion { get; init; } = BinaryFormatConstants.LatestVersion;
    public bool AllowV0Fallback { get; init; } = false;
}