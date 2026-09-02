namespace Engine.Serialization.Binary.Codec;

internal interface IFormatCodec
{
    int Version { get; }

    void Serialize<T>(Stream destination, T data) where T : class;
    T? Deserialize<T>(Stream source) where T : class;
}