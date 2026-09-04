using Engine.Serialization.Binary.Checksum;
using Engine.Serialization.Binary.Compression;
using Engine.Serialization.Binary.Encryption;

namespace Engine.Serialization.Binary.Configuration;

public static class AlgorithmResolver
{
    public static ICompressor ResolveCompressor(ICompressionAlgorithm? algorithm = null) =>
        new Compressor(algorithm ?? new NoCompression());

    public static ICompressor ResolveCompressor(CompressionAlgorithm kind, string? customName = null) =>
        new Compressor(CompressionAlgorithmRegistry.Resolve(kind, customName));

    public static IChecksumCalculator ResolveChecksum(IChecksumAlgorithm? algorithm = null) =>
        new ChecksumCalculator(algorithm ?? new NoChecksum());

    public static IChecksumCalculator ResolveChecksum(ChecksumAlgorithm kind, string? customName = null) =>
        new ChecksumCalculator(ChecksumAlgorithmRegistry.Resolve(kind, customName));

    public static IEncryptor ResolveEncryptor(IEncryptionAlgorithm? algorithm = null, byte[]? key = null, string? keyId = null) =>
        algorithm is null || algorithm.Kind == EncryptionAlgorithm.None
            ? Encryptor.None()
            : new Encryptor(algorithm, key, keyId);

    public static IEncryptor ResolveEncryptor(EncryptionAlgorithm kind, string? customName = null, byte[]? key = null, string? keyId = null)
    {
        if (kind == EncryptionAlgorithm.None) return Encryptor.None();
        var algorithm = EncryptionAlgorithmRegistry.Resolve(kind, customName);
        return new Encryptor(algorithm, key, keyId);
    }
}