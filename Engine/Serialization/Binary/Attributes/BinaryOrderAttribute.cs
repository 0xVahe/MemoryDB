namespace Engine.Serialization.Binary.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class BinaryOrderAttribute(int order) : Attribute
{
    public int Order { get; } = order;
}