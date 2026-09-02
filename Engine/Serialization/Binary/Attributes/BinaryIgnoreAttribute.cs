namespace Engine.Serialization.Binary.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class BinaryIgnoreAttribute : Attribute;