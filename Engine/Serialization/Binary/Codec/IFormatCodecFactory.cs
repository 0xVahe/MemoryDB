namespace Engine.Serialization.Binary.Codec;

internal interface IFormatCodecFactory
{
    int Version { get; }
    IFormatCodec Create(BinarySerializerOptions options);
}