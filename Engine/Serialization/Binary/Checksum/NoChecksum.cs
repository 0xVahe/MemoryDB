namespace Engine.Serialization.Binary.Checksum;

public sealed class NoChecksum : IChecksumAlgorithm
{
    public ChecksumAlgorithm Kind => ChecksumAlgorithm.None;
    public string? CustomName => null;
    public byte[] Compute(ReadOnlySpan<byte> data) => [];
}