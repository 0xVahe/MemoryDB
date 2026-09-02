using Engine.Serialization.Binary.Exceptions;

namespace Engine.Serialization.Binary.Encryption;

public sealed class Encryptor(
    IEncryptionAlgorithm defaultStrategy,
    IKeyResolver? keyResolver,
    string? defaultKeyId) : IEncryptor
{
    private readonly IEncryptionAlgorithm _defaultStrategy = defaultStrategy;
    private readonly IKeyResolver? _keyResolver = keyResolver;

    public EncryptionAlgorithm DefaultKind => _defaultStrategy.Kind;
    public string DefaultKeyId { get; } = defaultKeyId ?? string.Empty;

    public static Encryptor None() => new(new NoEncryption(), keyResolver: null, defaultKeyId: null);

    public byte[] Encrypt(byte[] plaintext)
    {
        if (DefaultKind == EncryptionAlgorithm.None) return plaintext;

        if (_keyResolver is null)
            throw new BinaryFormatValidationException("Key resolver is not configured.");

        if (string.IsNullOrWhiteSpace(DefaultKeyId))
            throw new BinaryFormatValidationException("DefaultKeyId is required when encryption is enabled.");

        var key = _keyResolver.Resolve(DefaultKeyId);
        return _defaultStrategy.Encrypt(plaintext, key);
    }

    public byte[] Decrypt(EncryptionAlgorithm kind, string? keyId, byte[] ciphertext, int expectedPlaintextLength)
    {
        if (kind == EncryptionAlgorithm.None) return ciphertext;

        if (_keyResolver is null)
            throw new BinaryFormatValidationException("Key resolver is not configured.");

        if (string.IsNullOrWhiteSpace(keyId))
            throw new BinaryFormatValidationException("Encrypted payload requires key id in header.");

        var strategy = EncryptionResolver.Resolve(kind);
        var key = _keyResolver.Resolve(keyId);
        return strategy.Decrypt(ciphertext, key, expectedPlaintextLength);
    }
}