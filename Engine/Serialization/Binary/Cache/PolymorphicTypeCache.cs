using System.Collections.Concurrent;
using Engine.Serialization.Binary.Attributes;

namespace Engine.Serialization.Binary.Cache;

internal static class PolymorphicTypeCache
{
    private static readonly ConcurrentDictionary<Type, PolymorphicMap?> Cache = new();

    public static PolymorphicMap? GetMap(Type declaredType) => Cache.GetOrAdd(declaredType, BuildMap);

    private static PolymorphicMap? BuildMap(Type declaredType)
    {
        var attrs = declaredType.GetCustomAttributes(typeof(BinaryKnownTypeAttribute), inherit: false)
            .Cast<BinaryKnownTypeAttribute>()
            .ToArray();

        if (attrs.Length == 0) return null;

        var knownTypes = attrs.Select(a => a.DerivedType).Distinct().ToArray();

        foreach (var t in knownTypes)
        {
            if (!declaredType.IsAssignableFrom(t))
                throw new InvalidOperationException($"Known type '{t}' is not assignable to '{declaredType}'.");
        }

        var byType = new Dictionary<Type, int>();
        var byId = new Dictionary<int, Type>();

        for (int i = 0; i < knownTypes.Length; i++)
        {
            int id = i + 1;
            byType[knownTypes[i]] = id;
            byId[id] = knownTypes[i];
        }

        return new PolymorphicMap(byType, byId);
    }
}

internal sealed class PolymorphicMap(
    IReadOnlyDictionary<Type, int> discriminatorByType,
    IReadOnlyDictionary<int, Type> typeByDiscriminator)
{
    public bool TryGetDiscriminator(Type runtimeType, out int discriminator) =>
        discriminatorByType.TryGetValue(runtimeType, out discriminator);

    public bool TryGetType(int discriminator, out Type? runtimeType)
    {
        var ok = typeByDiscriminator.TryGetValue(discriminator, out var t);
        runtimeType = t;
        return ok;
    }
}