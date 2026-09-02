namespace Engine.Serialization.Binary.Checksum;

public interface IChecksumCalculator
{
    ChecksumAlgorithm DefaultKind { get; }
    
    byte[] Compute(byte[] rawPayload);
    void Verify(ChecksumAlgorithm kind, byte[] rawPayload, byte[] expectedChecksum);
}