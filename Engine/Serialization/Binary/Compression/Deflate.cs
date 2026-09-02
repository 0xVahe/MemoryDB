using System.IO.Compression;

namespace Engine.Serialization.Binary.Compression;

public sealed class Deflate(CompressionLevel level = CompressionLevel.Optimal) : ICompressionAlgorithm
{
    public CompressionAlgorithm Kind => CompressionAlgorithm.Deflate;
    public string? CustomName => null;
    public Stream Wrap(Stream destination) => new DeflateStream(destination, level, leaveOpen: true);
    public Stream Unwrap(Stream source) => new DeflateStream(source, CompressionMode.Decompress, leaveOpen: true);
}