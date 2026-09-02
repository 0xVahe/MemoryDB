using Engine.Serialization.Binary.Exceptions;

namespace Engine.Serialization.Binary.Compression;

public sealed class Compressor(ICompressionAlgorithm defaultAlgorithm) : ICompressor
{
    private readonly ICompressionAlgorithm _defaultAlgorithm = defaultAlgorithm;

    public CompressionAlgorithm DefaultKind => _defaultAlgorithm.Kind;
    public string? DefaultCustomName => _defaultAlgorithm.CustomName;

    public static Compressor None() => new(new NoCompression());
    
    public byte[] Compress(byte[] rawPayload)
    {
        if (_defaultAlgorithm.Kind == CompressionAlgorithm.None) return rawPayload;

        using var output = new MemoryStream();
        using (var compressingStream = _defaultAlgorithm.Wrap(output))
            compressingStream.Write(rawPayload, 0, rawPayload.Length);

        return output.ToArray();
    }

    public byte[] Decompress(CompressionAlgorithm kind, string? customName, byte[] compressedPayload, int uncompressedLength)
    {
        if (kind == CompressionAlgorithm.None) return compressedPayload;

        var algorithm = CompressionAlgorithmRegistry.Resolve(kind, customName);

        using var input = new MemoryStream(compressedPayload);
        using var decompressingStream = algorithm.Unwrap(input);

        var output = new byte[uncompressedLength];
        int totalRead = 0;

        while (totalRead < uncompressedLength)
        {
            int read = decompressingStream.Read(output, totalRead, uncompressedLength - totalRead);
            if (read == 0) break;
            totalRead += read;
        }

        if (totalRead != uncompressedLength)
            throw new BinaryFormatException($"Decompression ended early. Expected {uncompressedLength} bytes, got {totalRead}.");

        return output;
    }
}