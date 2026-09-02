using Engine.Serialization.Binary.Exceptions;

namespace Engine.Serialization.Binary.Checksum;

public sealed class ChecksumCalculator(IChecksumAlgorithm defaultAlgorithm) : IChecksumCalculator
{
    private readonly IChecksumAlgorithm _defaultAlgorithm = defaultAlgorithm;

    public ChecksumAlgorithm DefaultKind => _defaultAlgorithm.Kind;
    public string? DefaultCustomName => _defaultAlgorithm.CustomName;
    
    public static ChecksumCalculator None() => new(new NoChecksum());

    public byte[] Compute(byte[] rawPayload) => _defaultAlgorithm.Compute(rawPayload);

    public void Verify(ChecksumAlgorithm kind, string? customName, byte[] rawPayload, byte[] expectedChecksum)
    {
        var algorithm = ChecksumAlgorithmRegistry.Resolve(kind, customName);
        byte[] actual = algorithm.Compute(rawPayload);

        if (!actual.AsSpan().SequenceEqual(expectedChecksum))
            throw new BinaryIntegrityException("Checksum mismatch — the binary payload appears to be corrupted.");
    }
}