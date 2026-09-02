namespace Engine.Serialization.Binary.Utils;

internal sealed class TypeShape
{
    public required FieldKind Kind { get; init; }
    public Type? ElementType { get; init; }
    public Type? UnderlyingType { get; init; }
    public Type? KeyType { get; init; }
    public Type? ValueType { get; init; }

    public static TypeShape Primitive() => new() { Kind = FieldKind.Primitive };
    public static TypeShape String() => new() { Kind = FieldKind.String };
    public static TypeShape Guid() => new() { Kind = FieldKind.Guid };
    public static TypeShape DateTime() => new() { Kind = FieldKind.DateTime };
    public static TypeShape TimeSpan() => new() { Kind = FieldKind.TimeSpan };
    public static TypeShape Enum(Type underlyingType) => new() { Kind = FieldKind.Enum, UnderlyingType = underlyingType };
    public static TypeShape Nullable(Type underlyingType) => new() { Kind = FieldKind.Nullable, UnderlyingType = underlyingType };
    public static TypeShape Array(Type elementType) => new() { Kind = FieldKind.Array, ElementType = elementType };
    public static TypeShape List(Type elementType) => new() { Kind = FieldKind.List, ElementType = elementType };
    public static TypeShape Dictionary(Type keyType, Type valueType) => new() { Kind = FieldKind.Dictionary, KeyType = keyType, ValueType = valueType };
    public static TypeShape Nested() => new() { Kind = FieldKind.Nested };
}