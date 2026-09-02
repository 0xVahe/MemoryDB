namespace Engine.Serialization.Binary.Encryption;

internal static class EncryptionResolver
{
    public static IEncryptionAlgorithm Resolve(EncryptionAlgorithm kind) => kind switch
    {
        EncryptionAlgorithm.None => new NoEncryption(),
        EncryptionAlgorithm.Aes256Gcm => new Aes256Gcm(),
        _ => throw new NotSupportedException($"Unknown encryption kind in file header: {kind}")
    };
}