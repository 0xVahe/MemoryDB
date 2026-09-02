using Engine.Serialization.Binary.Codec;
using Engine.Serialization.Binary.Versioning;

namespace Engine.Serialization.Binary;

public sealed class BinarySerializer
{
    private readonly BinaryFormatRouter _router;
    private readonly BinarySerializerOptions _options;

    public BinarySerializer(BinarySerializerOptions? options = null)
    {
        _options = options ?? BinarySerializerOptions.Default;

        var v1 = new V1FormatCodec(
            _options.Compressor,
            _options.Checksum,
            _options.Encryptor);

        var codecs = new List<IFormatCodec> { v1 };

        if (_options.AllowV0Fallback)
            codecs.Add(new V0FormatCodec());
        
        _router = new BinaryFormatRouter(codecs);
    }

    public byte[] Serialize<T>(T data) where T : class
    {
        using var ms = new MemoryStream();
        Serialize(ms, data);
        return ms.ToArray();
    }

    public void Serialize<T>(Stream destination, T data) where T : class =>
        _router.Serialize(destination, data, _options.WriteVersion);

    public T? Deserialize<T>(byte[] bytes) where T : class
    {
        ArgumentNullException.ThrowIfNull(bytes);
        if (bytes.Length == 0) return null;

        using var ms = new MemoryStream(bytes);
        return Deserialize<T>(ms);
    }

    public T? Deserialize<T>(Stream source) where T : class => 
        _router.Deserialize<T>(source);
}