using Engine.Serialization.Binary.Codec;
using Engine.Serialization.Binary.Exceptions;
using Engine.Serialization.Binary.Format;

namespace Engine.Serialization.Binary.Versioning;

internal sealed class BinaryFormatRouter(IEnumerable<IFormatCodec> codecs)
{
    private readonly Dictionary<int, IFormatCodec> _byVersion = codecs.ToDictionary(c => c.Version);

    public void Serialize<T>(Stream destination, T data, int version) where T : class
    {
        if (!_byVersion.TryGetValue(version, out var codec))
            throw new BinaryFormatNotSupportedException($"No codec registered for version {version}.");

        codec.Serialize(destination, data);
    }

    public T? Deserialize<T>(Stream source) where T : class
    {
        ArgumentNullException.ThrowIfNull(source);

        if (!source.CanSeek)
            throw new NotSupportedException($"{nameof(BinaryFormatRouter)} needs a seekable stream to detect the format version.");

        int version;

        if (BinaryHeaderPeek.TryPeekMagicAndVersion(source, out int detectedVersion))
        {
            version = detectedVersion;
        }
        else
        {
            if (_byVersion.ContainsKey(0))
                version = 0;
            else
                throw new BinaryFormatException("Not a recognized BinarySerializer stream (magic number mismatch and V0 fallback is disabled).");
        }

        if (!_byVersion.TryGetValue(version, out var codec))
            throw new BinaryFormatNotSupportedException($"No codec registered for version {version}.");

        return codec.Deserialize<T>(source);
    }
}