using System.Collections;

namespace Engine.Serialization.Binary.Utils;

internal static class FieldKindClassifier
{
    public static TypeShape Classify(Type type)
    {
        if (type == typeof(string)) return TypeShape.String();
        if (type == typeof(Guid)) return TypeShape.Guid();
        if (type == typeof(DateTime)) return TypeShape.DateTime();
        if (type == typeof(TimeSpan)) return TypeShape.TimeSpan();

        var nullableUnderlying = Nullable.GetUnderlyingType(type);
        if (nullableUnderlying is not null) return TypeShape.Nullable(nullableUnderlying);

        if (type.IsEnum) return TypeShape.Enum(Enum.GetUnderlyingType(type));
        if (type.IsPrimitive || type == typeof(decimal)) return TypeShape.Primitive();

        if (type.IsArray) return TypeShape.Array(type.GetElementType()!);

        if (TryGetDictionaryTypes(type, out var keyType, out var valueType))
            return TypeShape.Dictionary(keyType!, valueType!);

        if (type.IsGenericType && typeof(IEnumerable).IsAssignableFrom(type))
        {
            var elementType = type.GetGenericArguments().FirstOrDefault() ?? typeof(object);
            return TypeShape.Collection(elementType);
        }

        return TypeShape.Nested();
    }

    private static bool TryGetDictionaryTypes(Type type, out Type? keyType, out Type? valueType)
    {
        if (type.IsGenericType)
        {
            var def = type.GetGenericTypeDefinition();
            if (def == typeof(Dictionary<,>) || def == typeof(IDictionary<,>))
            {
                var args = type.GetGenericArguments();
                keyType = args[0];
                valueType = args[1];
                return true;
            }
        }

        var idict = type
            .GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>));

        if (idict is not null)
        {
            var args = idict.GetGenericArguments();
            keyType = args[0];
            valueType = args[1];
            return true;
        }

        keyType = null;
        valueType = null;
        return false;
    }
}