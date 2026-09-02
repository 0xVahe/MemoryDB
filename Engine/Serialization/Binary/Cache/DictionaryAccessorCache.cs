using System.Collections.Concurrent;
using System.Linq.Expressions;
using Engine.Serialization.Binary.Exceptions;

namespace Engine.Serialization.Binary.Cache;

internal static class DictionaryAccessorCache
{
    private static readonly ConcurrentDictionary<Type, EntryAccessors> EntryAccessorByType = new();
    private static readonly ConcurrentDictionary<(Type KeyType, Type ValueType), DictionaryAccessors> DictionaryAccessorByTypes = new();

    public static EntryAccessors GetEntryAccessors(Type entryType) =>
        EntryAccessorByType.GetOrAdd(entryType, BuildEntryAccessors);

    public static DictionaryAccessors GetDictionaryAccessors(Type keyType, Type valueType) =>
        DictionaryAccessorByTypes.GetOrAdd((keyType, valueType), static pair => BuildDictionaryAccessors(pair.KeyType, pair.ValueType));

    private static EntryAccessors BuildEntryAccessors(Type entryType)
    {
        var keyProp = entryType.GetProperty("Key")
            ?? throw new BinaryTypeException($"Dictionary entry type '{entryType}' does not expose Key.");

        var valueProp = entryType.GetProperty("Value")
            ?? throw new BinaryTypeException($"Dictionary entry type '{entryType}' does not expose Value.");

        var entryParam = Expression.Parameter(typeof(object), "entry");
        var typedEntry = Expression.Convert(entryParam, entryType);

        var keyExpr = Expression.Convert(Expression.Property(typedEntry, keyProp), typeof(object));
        var valueExpr = Expression.Convert(Expression.Property(typedEntry, valueProp), typeof(object));

        var keyGetter = Expression.Lambda<Func<object, object?>>(keyExpr, entryParam).Compile();
        var valueGetter = Expression.Lambda<Func<object, object?>>(valueExpr, entryParam).Compile();

        return new EntryAccessors(keyGetter, valueGetter);
    }

    private static DictionaryAccessors BuildDictionaryAccessors(Type keyType, Type valueType)
    {
        var dictType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);

        var factory = Expression.Lambda<Func<object>>(Expression.Convert(Expression.New(dictType), typeof(object))).Compile();

        var dictParam = Expression.Parameter(typeof(object), "dict");
        var keyParam = Expression.Parameter(typeof(object), "key");
        var valueParam = Expression.Parameter(typeof(object), "value");

        var typedDict = Expression.Convert(dictParam, dictType);
        var typedKey = Expression.Convert(keyParam, keyType);
        var typedValue = Expression.Convert(valueParam, valueType);

        var addMethod = dictType.GetMethod("Add", [keyType, valueType])
            ?? throw new BinaryTypeException($"Method Add({keyType.Name}, {valueType.Name}) was not found on {dictType}.");

        var call = Expression.Call(typedDict, addMethod, typedKey, typedValue);
        var add = Expression.Lambda<Action<object, object?, object?>>(call, dictParam, keyParam, valueParam).Compile();

        return new DictionaryAccessors(factory, add);
    }

    internal sealed record EntryAccessors(
        Func<object, object?> KeyGetter,
        Func<object, object?> ValueGetter);

    internal sealed record DictionaryAccessors(
        Func<object> CreateInstance,
        Action<object, object?, object?> Add);
}