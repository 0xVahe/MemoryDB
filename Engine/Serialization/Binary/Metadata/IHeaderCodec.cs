namespace Engine.Serialization.Binary.Metadata;

internal interface IHeaderCodec
{
    int Version { get; }
    int Magic { get; }

    object Read(BinaryReader reader);
    void Write(BinaryWriter writer, object header);
}