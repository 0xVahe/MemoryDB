using System.Collections.Concurrent;
using Engine.Serialization.Binary.Exceptions;

namespace Engine.Serialization.Binary.Encryption;

public sealed class InMemoryKeyResolver(ConcurrentDictionary<string, byte[]> keys) : IKeyResolver
{
    private readonly ConcurrentDictionary<string, byte[]> _keys = keys;

    public byte[] Resolve(string keyId)
    {
        if (string.IsNullOrWhiteSpace(keyId))
            throw new BinaryFormatValidationException("Encryption key id is missing.");

        if (!_keys.TryGetValue(keyId, out var key))
            throw new BinaryFormatValidationException($"Encryption key '{keyId}' was not found.");

        return key;
    }
}