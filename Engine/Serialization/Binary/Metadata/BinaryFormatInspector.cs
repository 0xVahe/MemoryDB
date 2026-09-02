using System.Text;
using Engine.Serialization.Binary.Format;

namespace Engine.Serialization.Binary.Metadata;

public static class BinaryFormatInspector
{
    public static BinaryHeaderInfo? Peek(Stream source)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (!source.CanSeek)
            throw new NotSupportedException($"{nameof(Peek)} needs a seekable stream.");

        long start = source.Position;
        try
        {
            if (!BinaryHeaderPeek.TryPeekMagicAndVersion(source, out int version)) return null;
            if (version != BinaryFormatConstants.LatestVersion) return null;

            using var reader = new BinaryReader(source, Encoding.UTF8, leaveOpen: true);
            var header = BinaryFormatHeaderV1.ReadFrom(reader);
            return new BinaryHeaderInfo(header.FormatVersion, header.Compression, header.ChecksumAlgorithm, header.Encryption, header.KeyId);
        }
        finally
        {
            source.Position = start;
        }
    }
}
