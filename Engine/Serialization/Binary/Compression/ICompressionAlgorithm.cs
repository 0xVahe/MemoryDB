namespace Engine.Serialization.Binary.Compression;

public interface ICompressionAlgorithm
{
    CompressionAlgorithm Kind { get; }
    string? CustomName { get; }
    Stream Wrap(Stream destination);
    Stream Unwrap(Stream source);
}