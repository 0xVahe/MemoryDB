namespace Engine.Serialization.Binary.Checksum;

public interface IChecksumAlgorithm
{
    ChecksumAlgorithm Kind { get; }
    
    byte[] Compute(ReadOnlySpan<byte> data);
}