namespace Engine.Serialization.Binary.Checksum;

internal static class ChecksumResolver
{
    public static IChecksumAlgorithm Resolve(ChecksumAlgorithm kind) => kind switch
    {
        ChecksumAlgorithm.None => new NoChecksum(),
        ChecksumAlgorithm.Crc32 => new Crc32(),
        _ => throw new NotSupportedException($"Unknown checksum kind in file header: {kind}")
    };
}