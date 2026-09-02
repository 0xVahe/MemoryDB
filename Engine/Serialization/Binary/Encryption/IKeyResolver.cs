namespace Engine.Serialization.Binary.Encryption;

public interface IKeyResolver
{
    byte[] Resolve(string keyId);
}