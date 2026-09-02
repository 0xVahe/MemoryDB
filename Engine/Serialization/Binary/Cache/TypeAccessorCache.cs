using System.Linq.Expressions;
using System.Reflection;
using System.Collections.Concurrent;
using Engine.Serialization.Binary.Attributes;
using Engine.Serialization.Binary.Utils;

namespace Engine.Serialization.Binary.Cache;

internal static class TypeAccessorCache
{
    private static readonly ConcurrentDictionary<Type, TypeAccessorPlan> Cache = [];

    public static TypeAccessorPlan GetOrBuild(Type type)
    {
        if (Cache.TryGetValue(type, out var plan)) return plan;

        plan = BuildPlan(type);
        Cache[type] = plan;
        return plan;
    }

    private static TypeAccessorPlan BuildPlan(Type type)
    {
        var propertyMembers = type
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.CanWrite && p.GetIndexParameters().Length == 0)
            .Where(p => p.GetCustomAttribute<BinaryIgnoreAttribute>() is null)
            .Select(p => new MemberSource(
                p.Name,
                p.GetCustomAttribute<BinaryOrderAttribute>()?.Order ?? int.MaxValue,
                BuildAccessor(p)));

        var fieldMembers = type
            .GetFields(BindingFlags.Public | BindingFlags.Instance)
            .Where(f => !f.IsStatic)
            .Where(f => f.GetCustomAttribute<BinaryIgnoreAttribute>() is null)
            .Select(f => new MemberSource(
                f.Name,
                f.GetCustomAttribute<BinaryOrderAttribute>()?.Order ?? int.MaxValue,
                BuildAccessor(f)));

        var members = propertyMembers
            .Concat(fieldMembers)
            .OrderBy(m => m.Order)
            .ThenBy(m => m.Name, StringComparer.Ordinal)
            .Select(m => m.Accessor)
            .ToArray();

        return new TypeAccessorPlan
        {
            Type = type,
            Members = members
        };
    }

    private static MemberAccessor BuildAccessor(PropertyInfo property)
    {
        var shape = FieldKindClassifier.Classify(property.PropertyType);

        return new MemberAccessor
        {
            Name = property.Name,
            MemberType = property.PropertyType,
            Kind = shape.Kind,
            ElementType = shape.ElementType,
            UnderlyingType = shape.UnderlyingType,
            KeyType = shape.KeyType,
            ValueType = shape.ValueType,
            Getter = BuildPropertyGetter(property),
            Setter = BuildPropertySetter(property)
        };
    }

    private static MemberAccessor BuildAccessor(FieldInfo field)
    {
        var shape = FieldKindClassifier.Classify(field.FieldType);

        return new MemberAccessor
        {
            Name = field.Name,
            MemberType = field.FieldType,
            Kind = shape.Kind,
            ElementType = shape.ElementType,
            UnderlyingType = shape.UnderlyingType,
            KeyType = shape.KeyType,
            ValueType = shape.ValueType,
            Getter = BuildFieldGetter(field),
            Setter = BuildFieldSetter(field)
        };
    }

    private static Func<object, object?> BuildPropertyGetter(PropertyInfo property)
    {
        var instanceParam = Expression.Parameter(typeof(object), "instance");
        var typedInstance = Expression.Convert(instanceParam, property.DeclaringType!);
        var propertyAccess = Expression.Property(typedInstance, property);
        var boxedResult = Expression.Convert(propertyAccess, typeof(object));

        return Expression.Lambda<Func<object, object?>>(boxedResult, instanceParam).Compile();
    }

    private static Action<object, object?> BuildPropertySetter(PropertyInfo property)
    {
        var instanceParam = Expression.Parameter(typeof(object), "instance");
        var valueParam = Expression.Parameter(typeof(object), "value");

        var typedInstance = Expression.Convert(instanceParam, property.DeclaringType!);
        var typedValue = Expression.Convert(valueParam, property.PropertyType);
        var propertyAccess = Expression.Property(typedInstance, property);
        var assign = Expression.Assign(propertyAccess, typedValue);

        return Expression.Lambda<Action<object, object?>>(assign, instanceParam, valueParam).Compile();
    }

    private static Func<object, object?> BuildFieldGetter(FieldInfo field)
    {
        var instanceParam = Expression.Parameter(typeof(object), "instance");
        var typedInstance = Expression.Convert(instanceParam, field.DeclaringType!);
        var fieldAccess = Expression.Field(typedInstance, field);
        var boxedResult = Expression.Convert(fieldAccess, typeof(object));

        return Expression.Lambda<Func<object, object?>>(boxedResult, instanceParam).Compile();
    }

    private static Action<object, object?> BuildFieldSetter(FieldInfo field)
    {
        var instanceParam = Expression.Parameter(typeof(object), "instance");
        var valueParam = Expression.Parameter(typeof(object), "value");

        var typedInstance = Expression.Convert(instanceParam, field.DeclaringType!);
        var typedValue = Expression.Convert(valueParam, field.FieldType);
        var fieldAccess = Expression.Field(typedInstance, field);
        var assign = Expression.Assign(fieldAccess, typedValue);

        return Expression.Lambda<Action<object, object?>>(assign, instanceParam, valueParam).Compile();
    }

    public static int CachedTypeCount => Cache.Count;

    private sealed record MemberSource(string Name, int Order, MemberAccessor Accessor);
}