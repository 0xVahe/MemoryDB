namespace Engine.Serialization.Binary;

public static class StreamExtensions
{
    public static void Serialize<T>(this Stream destination, T data, BinarySerializerOptions? options = null) where T : class
    {
        ArgumentNullException.ThrowIfNull(destination);

        var serializer = new BinarySerializer(options ?? BinarySerializerOptions.Default);
        serializer.Serialize(destination, data);
    }
    
    public static T? Deserialize<T>(this Stream source, byte[]? key) where T : class
    {
        ArgumentNullException.ThrowIfNull(source);

        var options = BinarySerializerOptions.FromStream(source, key);
        var serializer = new BinarySerializer(options);

        return serializer.Deserialize<T>(source);
    }

    public static T? Deserialize<T>(this Stream source, Func<string?, byte[]?>? keyResolver = null) where T : class
    {
        ArgumentNullException.ThrowIfNull(source);

        var options = BinarySerializerOptions.FromStream(source, keyResolver ?? (_ => null));
        var serializer = new BinarySerializer(options);

        return serializer.Deserialize<T>(source);
    }
}