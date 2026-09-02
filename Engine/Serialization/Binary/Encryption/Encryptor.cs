namespace Engine.Serialization.Binary.Encryption;

public sealed class Encryptor(IEncryptionAlgorithm defaultAlgorithm, byte[]? key = null, string? keyId = null) : IEncryptor
{
    private readonly IEncryptionAlgorithm _defaultAlgorithm = defaultAlgorithm;
    private readonly byte[] _key = key ?? [];

    public EncryptionAlgorithm DefaultKind => _defaultAlgorithm.Kind;
    public string? DefaultCustomName => _defaultAlgorithm.CustomName;
    public string? DefaultKeyId { get; } = keyId;

    public static Encryptor None() => new(new NoEncryption());

    public byte[] Encrypt(byte[] plaintext) => _defaultAlgorithm.Encrypt(plaintext, _key);

    public byte[] Decrypt(EncryptionAlgorithm kind, string? customName, byte[] ciphertext, int expectedPlaintextLength)
    {
        if (kind == EncryptionAlgorithm.None) return ciphertext;

        var algorithm = EncryptionAlgorithmRegistry.Resolve(kind, customName);
        return algorithm.Decrypt(ciphertext, _key, expectedPlaintextLength);
    }
}