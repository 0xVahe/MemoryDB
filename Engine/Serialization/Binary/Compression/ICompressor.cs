namespace Engine.Serialization.Binary.Compression;

public interface ICompressor
{
    CompressionAlgorithm DefaultKind { get; }

    byte[] Compress(byte[] rawPayload);
    byte[] Decompress(CompressionAlgorithm kind, byte[] compressedPayload, int uncompressedLength);
}