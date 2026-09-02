namespace Engine.Serialization.Binary.Utils;

internal sealed class MemberAccessor
{
    public required string Name { get; init; }
    public required Type MemberType { get; init; }
    public required FieldKind Kind { get; init; }
    public Type? ElementType { get; init; }
    public Type? UnderlyingType { get; init; }
    public Type? KeyType { get; init; }
    public Type? ValueType { get; init; }

    public required Func<object, object?> Getter { get; init; }
    public required Action<object, object?> Setter { get; init; }
}