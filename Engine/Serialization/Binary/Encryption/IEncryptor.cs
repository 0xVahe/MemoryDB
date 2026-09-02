namespace Engine.Serialization.Binary.Encryption;

public interface IEncryptor
{
    EncryptionAlgorithm DefaultKind { get; }
    string DefaultKeyId { get; }

    byte[] Encrypt(byte[] plaintext);

    byte[] Decrypt(
        EncryptionAlgorithm kind,
        string? keyId,
        byte[] ciphertext,
        int expectedPlaintextLength);
}