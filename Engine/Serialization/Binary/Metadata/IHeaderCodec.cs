namespace Engine.Serialization.Binary.Metadata;

internal interface IHeaderCodec
{
    int Version { get; }
    BinaryHeaderInfo ReadHeaderInfo(BinaryReader reader);
}