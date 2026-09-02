namespace Engine.Serialization.Binary.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class BinaryKnownTypeAttribute(Type derivedType) : Attribute
{
    public Type DerivedType { get; } = derivedType;
}