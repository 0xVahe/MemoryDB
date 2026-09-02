namespace Engine.Serialization.Binary.Checksum;

public sealed class ChecksumCalculator(IChecksumAlgorithm defaultAlgorithm) : IChecksumCalculator
{
    private readonly IChecksumAlgorithm _defaultAlgorithm = defaultAlgorithm;

    public ChecksumAlgorithm DefaultKind => _defaultAlgorithm.Kind;

    public byte[] Compute(byte[] rawPayload) => _defaultAlgorithm.Compute(rawPayload);

    public void Verify(ChecksumAlgorithm kind, byte[] rawPayload, byte[] expectedChecksum)
    {
        var algorithm = ChecksumResolver.Resolve(kind);
        byte[] actualChecksum = algorithm.Compute(rawPayload);

        if (!actualChecksum.AsSpan().SequenceEqual(expectedChecksum))
            throw new InvalidDataException("Checksum mismatch — the binary payload appears to be corrupted.");
    }
}