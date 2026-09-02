namespace Engine.Serialization.Binary.Checksum;

public interface IChecksumAlgorithm
{
    ChecksumAlgorithm Kind { get; }
    string? CustomName { get; }
    byte[] Compute(ReadOnlySpan<byte> data);
}