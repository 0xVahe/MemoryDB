using System.IO.Compression;

namespace Engine.Serialization.Binary.Compression;

public sealed class Brotli(CompressionLevel level = CompressionLevel.Optimal) : ICompressionAlgorithm
{
    public CompressionAlgorithm Kind => CompressionAlgorithm.Brotli;
    public string? CustomName => null;
    public Stream Wrap(Stream destination) => new BrotliStream(destination, level, leaveOpen: true);
    public Stream Unwrap(Stream source) => new BrotliStream(source, CompressionMode.Decompress, leaveOpen: true);
}