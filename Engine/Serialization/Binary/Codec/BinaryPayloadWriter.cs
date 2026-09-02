using System.Collections;
using Engine.Serialization.Binary.Cache;
using Engine.Serialization.Binary.Utils;

namespace Engine.Serialization.Binary.Codec;

internal sealed class BinaryPayloadWriter(BinaryWriter writer)
{
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
            if (!polymorphicMap.TryGetDiscriminator(runtimeType, out int discriminator))
                throw new InvalidDataException($"Runtime type '{runtimeType}' is not allowed for declared type '{declaredType}'.");

            writer.Write(discriminator);

            if (runtimeType != declaredType)
            {
                WriteNested(nonNullValue);
                return;
            }
        }

        var shape = FieldKindClassifier.Classify(declaredType);

        switch (shape.Kind)
        {
            case FieldKind.String:
                writer.Write((string)nonNullValue);
                break;
            case FieldKind.Guid:
                writer.Write(((Guid)nonNullValue).ToByteArray());
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
                WritePrimitive(value);
                break;
            case FieldKind.Array:
            case FieldKind.List:
                WriteCollection((IEnumerable)value, shape.ElementType!);
                break;
            case FieldKind.Dictionary:
                WriteDictionary((IEnumerable)value, shape.KeyType!, shape.ValueType!);
                break;
            case FieldKind.Nested:
                WriteNested(value);
                break;
            default:
                throw new NotSupportedException($"Type '{declaredType}' is not supported.");
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
                throw new NotSupportedException($"Unsupported primitive type: {value.GetType()}");
        }
    }

    private void WriteCollection(IEnumerable value, Type elementType)
    {
        var items = value.Cast<object>().ToList();
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
        var plan = TypeAccessorCache.GetOrBuild(value.GetType());
        foreach (var accessor in plan.Members)
            WriteValue(accessor.Getter(value), accessor.MemberType);
    }
}