using System.Collections;
using Engine.Serialization.Binary.Cache;
using Engine.Serialization.Binary.Exceptions;
using Engine.Serialization.Binary.Utils;

namespace Engine.Serialization.Binary.Codec;

internal sealed class BinaryPayloadWriter(BinaryWriter writer)
{
    private readonly HashSet<object> _activeAncestors = new(ReferenceEqualityComparer.Instance);
    public void Serialize<T>(T data) where T : class => WriteValue(data, typeof(T));

    private void WriteValue(object? value, Type declaredType)
    {
        bool canBeNull = !declaredType.IsValueType || Nullable.GetUnderlyingType(declaredType) != null;

        if (canBeNull) 
        {
            writer.Write(value != null);
        }
        
        if (value is not { } nonNullValue) 
            return;

        var polymorphicMap = PolymorphicTypeCache.GetMap(declaredType);
        if (polymorphicMap is not null)
        {
            var runtimeType = nonNullValue.GetType();
            if (!polymorphicMap.TryGetDiscriminator(runtimeType, out byte discriminator))
                throw new BinaryTypeException(
                    $"Runtime type '{runtimeType}' is not allowed for declared type '{declaredType}' — " +
                    $"add [BinaryKnownType(typeof({runtimeType.Name}))] on '{declaredType.Name}'.");

            writer.Write(discriminator);
            WriteNested(value);
            return;
        }

        var shape = FieldKindClassifier.Classify(declaredType);

        switch (shape.Kind)
        {
            case FieldKind.String:
                writer.Write((string)nonNullValue);
                break;
            case FieldKind.Guid:
                Span<byte> guidSpan = stackalloc byte[16];
                ((Guid)nonNullValue).TryWriteBytes(guidSpan);
                writer.Write(guidSpan);
                break;
            case FieldKind.DateTime:
                writer.Write(((DateTime)nonNullValue).ToBinary());
                break;
            case FieldKind.TimeSpan:
                writer.Write(((TimeSpan)nonNullValue).Ticks);
                break;
            case FieldKind.Enum:
                WritePrimitive(Convert.ChangeType(value, shape.UnderlyingType!));
                break;
            case FieldKind.Nullable:
                WriteValue(value, shape.UnderlyingType!);
                break;
            case FieldKind.Primitive:
                WritePrimitive(nonNullValue);
                break;
            case FieldKind.Array:
            case FieldKind.Collection:
                WriteCollection((IEnumerable)nonNullValue, shape.ElementType!);
                break;
            case FieldKind.Dictionary:
                WriteDictionary((IEnumerable)nonNullValue, shape.KeyType!, shape.ValueType!);
                break;
            case FieldKind.Nested:
                WriteNested(nonNullValue);
                break;
            default:
                throw new BinaryTypeException($"Type '{declaredType}' is not supported.");
        }
    }

    private void WritePrimitive(object value)
    {
        switch (value)
        {
            case bool v: writer.Write(v); break;
            case byte v: writer.Write(v); break;
            case sbyte v: writer.Write(v); break;
            case short v: writer.Write(v); break;
            case ushort v: writer.Write(v); break;
            case int v: writer.Write(v); break;
            case uint v: writer.Write(v); break;
            case long v: writer.Write(v); break;
            case ulong v: writer.Write(v); break;
            case float v: writer.Write(v); break;
            case double v: writer.Write(v); break;
            case decimal v: writer.Write(v); break;
            case char v: writer.Write(v); break;
            default:
                throw new BinaryTypeException($"Unsupported primitive type: {value.GetType()}");
        }
    }

    private void WriteCollection(IEnumerable value, Type elementType)
    {
        var items = value.Cast<object>().ToList();
        
        if (CollectionAccessorCache.ReverseOnWrite(value.GetType()))
            items.Reverse();

        writer.Write(items.Count);
        foreach (var item in items)
            WriteValue(item, elementType);
    }

    private void WriteDictionary(IEnumerable dictionaryEntries, Type keyType, Type valueType)
    {
        var entries = dictionaryEntries.Cast<object>().ToList();
        writer.Write(entries.Count);

        foreach (var entry in entries)
        {
            var accessors = DictionaryAccessorCache.GetEntryAccessors(entry.GetType());
            var key = accessors.KeyGetter(entry);
            var val = accessors.ValueGetter(entry);

            WriteValue(key, keyType);
            WriteValue(val, valueType);
        }
    }

    private void WriteNested(object value)
    {
        var type = value.GetType();
        bool tracksForCycles = !type.IsValueType;
        if (tracksForCycles && !_activeAncestors.Add(value))
            throw new BinaryTypeException(
                $"Circular reference detected while serializing '{type}' — an object " +
                "of this type refers back to an ancestor already being written. Circular object " +
                "graphs are not supported (no reference-preservation); break the cycle before " +
                "serializing, or exclude one side of it with [BinaryIgnore].");
        
        try
        {
            var plan = TypeAccessorCache.GetOrBuild(type);
            foreach (var accessor in plan.Members)
                WriteValue(accessor.Getter(value), accessor.MemberType);
        }
        finally
        {
            if (tracksForCycles) _activeAncestors.Remove(value);
        }
    }
}