namespace Engine.Serialization.Binary.Compression;

public sealed class NoCompression : ICompressionAlgorithm
{
    public CompressionAlgorithm Kind => CompressionAlgorithm.None;
    public string? CustomName => null;
    public Stream Wrap(Stream destination) => destination;
    public Stream Unwrap(Stream source) => source;
}