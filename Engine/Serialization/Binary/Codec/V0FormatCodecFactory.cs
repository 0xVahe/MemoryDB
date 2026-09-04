namespace Engine.Serialization.Binary.Codec;

internal sealed class V0FormatCodecFactory : IFormatCodecFactory
{
    public int Version => 0;

    public IFormatCodec Create(BinarySerializerOptions options) =>
        new V0FormatCodec();
}