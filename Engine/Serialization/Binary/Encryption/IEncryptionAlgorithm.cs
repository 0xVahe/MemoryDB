namespace Engine.Serialization.Binary.Encryption;

public interface IEncryptionAlgorithm
{
    EncryptionAlgorithm Kind { get; }

    byte[] Encrypt(byte[] plaintext, byte[] key);

    byte[] Decrypt(byte[] ciphertext, byte[] key, int expectedPlaintextLength);
}