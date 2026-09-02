namespace Engine.Serialization.Binary.Utils;

internal enum FieldKind
{
    Primitive,
    String,
    Guid,
    DateTime,
    TimeSpan,
    Enum,
    Nullable,
    Array,
    Collection,
    Dictionary,
    Nested
}