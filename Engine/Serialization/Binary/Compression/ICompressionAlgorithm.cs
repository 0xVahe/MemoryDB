namespace Engine.Serialization.Binary.Compression;

public interface ICompressionAlgorithm
{
    CompressionAlgorithm Kind { get; }
    Stream Wrap(Stream destination);
    Stream Unwrap(Stream source);
}