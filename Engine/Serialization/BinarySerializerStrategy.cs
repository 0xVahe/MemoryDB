using Core;
using Engine.Serialization.Binary;

namespace Engine.Serialization;

public sealed class BinarySerializerStrategy(BinarySerializer? engine = null) : IStorageSerializer
{
    private readonly BinarySerializer _engine = engine ?? new BinarySerializer();

    public byte[] Serialize<T>(T data) where T : class => _engine.Serialize(data);

    public T? Deserialize<T>(byte[] bytes) where T : class => _engine.Deserialize<T>(bytes);
}