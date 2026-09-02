using Engine.Serialization.Binary.Cache;
using Engine.Serialization.Binary.Exceptions;
using Engine.Serialization.Binary.Utils;

namespace Engine.Serialization.Binary.Codec;

internal sealed class BinaryPayloadReader(BinaryReader reader)
{
    public T? Deserialize<T>() where T : class => (T?)ReadValue(typeof(T));

    private object? ReadValue(Type declaredType)
    {
        bool canBeNull = !declaredType.IsValueType || Nullable.GetUnderlyingType(declaredType) != null;
        if (canBeNull)
        {
            bool hasValue = reader.ReadBoolean();
            if (!hasValue) return null;
        }

        var polymorphicMap = PolymorphicTypeCache.GetMap(declaredType);
        if (polymorphicMap is not null)
        {
            byte discriminator = reader.ReadByte();
            if (!polymorphicMap.TryGetType(discriminator, out var runtimeType) || runtimeType is null)
                throw new BinaryTypeException(
                    $"Unknown discriminator '{discriminator}' for declared type '{declaredType}' — the data may be from an incompatible version.");

            return ReadNested(runtimeType);
        }

        var shape = FieldKindClassifier.Classify(declaredType);

        return shape.Kind switch
        {
            FieldKind.String => reader.ReadString(),
            FieldKind.Guid => ReadGuid(),
            FieldKind.DateTime => DateTime.FromBinary(reader.ReadInt64()),
            FieldKind.TimeSpan => TimeSpan.FromTicks(reader.ReadInt64()),
            FieldKind.Enum => Enum.ToObject(declaredType, ReadPrimitive(shape.UnderlyingType!)),
            FieldKind.Nullable => ReadValue(shape.UnderlyingType!),
            FieldKind.Primitive => ReadPrimitive(declaredType),
            FieldKind.Array => ReadArray(shape.ElementType!),
            FieldKind.Collection => ReadCollection(declaredType, shape.ElementType!),
            FieldKind.Dictionary => ReadDictionary(shape.KeyType!, shape.ValueType!),
            FieldKind.Nested => ReadNested(declaredType),
            _ => throw new BinaryTypeException($"Type '{declaredType}' is not supported.")
        };
    }

    private Guid ReadGuid()
    {
        Span<byte> bytes = stackalloc byte[16];
        reader.ReadExactly(bytes);
        return new Guid(bytes);
    }
    
    private object ReadPrimitive(Type type)
    {
        if (type == typeof(bool)) return reader.ReadBoolean();
        if (type == typeof(byte)) return reader.ReadByte();
        if (type == typeof(sbyte)) return reader.ReadSByte();
        if (type == typeof(short)) return reader.ReadInt16();
        if (type == typeof(ushort)) return reader.ReadUInt16();
        if (type == typeof(int)) return reader.ReadInt32();
        if (type == typeof(uint)) return reader.ReadUInt32();
        if (type == typeof(long)) return reader.ReadInt64();
        if (type == typeof(ulong)) return reader.ReadUInt64();
        if (type == typeof(float)) return reader.ReadSingle();
        if (type == typeof(double)) return reader.ReadDouble();
        if (type == typeof(decimal)) return reader.ReadDecimal();
        if (type == typeof(char)) return reader.ReadChar();

        throw new BinaryTypeException($"Unsupported primitive type: {type}");
    }

    private Array ReadArray(Type elementType)
    {
        int count = reader.ReadInt32();
        var array = Array.CreateInstance(elementType, count);
        for (int i = 0; i < count; i++)
            array.SetValue(ReadValue(elementType), i);
        return array;
    }

    private object ReadCollection(Type declaredType, Type elementType)
    {
        int count = reader.ReadInt32();
        var accessors = CollectionAccessorCache.GetAccessors(declaredType, elementType);
        var instance = accessors.CreateInstance();

        for (int i = 0; i < count; i++)
            accessors.Add(instance, ReadValue(elementType));

        return instance;
    }

    private object ReadDictionary(Type keyType, Type valueType)
    {
        int count = reader.ReadInt32();

        var accessors = DictionaryAccessorCache.GetDictionaryAccessors(keyType, valueType);
        var dict = accessors.CreateInstance();

        for (int i = 0; i < count; i++)
        {
            var key = ReadValue(keyType);
            var value = ReadValue(valueType);
            accessors.Add(dict, key, value);
        }

        return dict;
    }

    private object ReadNested(Type type)
    {
        var instance = Activator.CreateInstance(type)!;
        var plan = TypeAccessorCache.GetOrBuild(type);

        foreach (var accessor in plan.Members)
            accessor.Setter(instance, ReadValue(accessor.MemberType));

        return instance;
    }
}