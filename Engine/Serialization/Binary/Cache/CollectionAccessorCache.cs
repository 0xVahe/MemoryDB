using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using Engine.Serialization.Binary.Exceptions;

namespace Engine.Serialization.Binary.Cache;

internal static class CollectionAccessorCache
{
    private static readonly ConcurrentDictionary<Type, CollectionAccessors> Cache = new();

    public static CollectionAccessors GetAccessors(Type declaredType, Type elementType) =>
        Cache.GetOrAdd(declaredType, _ => Build(declaredType, elementType));

    public static bool ReverseOnWrite(Type runtimeType) =>
        runtimeType.IsGenericType && runtimeType.GetGenericTypeDefinition() == typeof(Stack<>);

    private static CollectionAccessors Build(Type declaredType, Type elementType)
    {
        var concreteType = ResolveConcreteType(declaredType, elementType);
        var methodName = DetermineAddMethodName(concreteType);

        var addMethod = concreteType.GetMethod(methodName, [elementType])
            ?? throw new BinaryTypeException(
                $"Type '{concreteType}' has no compatible '{methodName}({elementType.Name})' method and can't be " +
                "reconstructed. Supported: ICollection<T>-implementing types (List, HashSet, SortedSet, LinkedList, " +
                "ObservableCollection, custom types with Add(T)), Stack<T>, Queue<T>. " +
                "System.Collections.Immutable.* types are not supported (Add returns a new instance instead of mutating).");

        return new CollectionAccessors(BuildFactory(concreteType), BuildAdd(concreteType, addMethod, elementType));
    }

    private static Type ResolveConcreteType(Type declaredType, Type elementType)
    {
        if (!declaredType.IsInterface && !declaredType.IsAbstract)
            return declaredType;

        if (declaredType.IsGenericType && declaredType.GetGenericTypeDefinition() == typeof(ISet<>))
            return typeof(HashSet<>).MakeGenericType(elementType);

        return typeof(List<>).MakeGenericType(elementType);
    }

    private static string DetermineAddMethodName(Type concreteType)
    {
        if (concreteType.IsGenericType)
        {
            var def = concreteType.GetGenericTypeDefinition();
            if (def == typeof(Stack<>)) return "Push";
            if (def == typeof(Queue<>)) return "Enqueue";
        }

        return "Add";
    }

    private static Func<object> BuildFactory(Type concreteType)
    {
        var newExpr = Expression.New(concreteType);
        return Expression.Lambda<Func<object>>(Expression.Convert(newExpr, typeof(object))).Compile();
    }

    private static Action<object, object?> BuildAdd(Type concreteType, MethodInfo addMethod, Type elementType)
    {
        var instanceParam = Expression.Parameter(typeof(object), "instance");
        var itemParam = Expression.Parameter(typeof(object), "item");
        var typedInstance = Expression.Convert(instanceParam, concreteType);
        var typedItem = Expression.Convert(itemParam, elementType);
        var call = Expression.Call(typedInstance, addMethod, typedItem);

        return Expression.Lambda<Action<object, object?>>(call, instanceParam, itemParam).Compile();
    }

    internal sealed record CollectionAccessors(
        Func<object> CreateInstance,
        Action<object, object?> Add);
}