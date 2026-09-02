namespace Engine.Serialization.Binary.Compression;

internal static class CompressionResolver
{
    public static ICompressionAlgorithm Resolve(CompressionAlgorithm kind) => kind switch
    {
        CompressionAlgorithm.None => new NoCompression(),
        CompressionAlgorithm.Deflate => new Deflate(),
        CompressionAlgorithm.Brotli => new Brotli(),
        _ => throw new NotSupportedException($"Unknown compression kind in file header: {kind}")
    };
}