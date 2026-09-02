namespace Engine.Serialization.Binary.Metadata;

internal interface IHeaderValidator
{
    int Version { get; }
    void Validate(object header);
}