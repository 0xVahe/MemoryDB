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
            throw new BinaryFormatValidationException($"No codec registered for version {version}.");

        codec.Serialize(destination, data);
    }

    public T? Deserialize<T>(Stream source) where T : class
    {
        long p = source.Position;
        using var reader = new BinaryReader(source, System.Text.Encoding.UTF8, leaveOpen: true);

        int magic = reader.ReadInt32();
        int version = reader.ReadInt32();
        source.Position = p;

        if (magic != BinaryFormatConstants.Magic)
            throw new BinaryFormatValidationException("Magic mismatch.");

        if (!_byVersion.TryGetValue(version, out var codec))
            throw new BinaryFormatValidationException($"No codec registered for version {version}.");

        return codec.Deserialize<T>(source);
    }
}