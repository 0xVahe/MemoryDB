namespace Engine.Serialization.Binary.Encryption;

public sealed class NoEncryption : IEncryptionAlgorithm
{
    public EncryptionAlgorithm Kind => EncryptionAlgorithm.None;
    public string? CustomName => null;
    public byte[] Encrypt(byte[] plaintext, byte[] key) => plaintext;
    public byte[] Decrypt(byte[] ciphertext, byte[] key, int expectedPlaintextLength) => ciphertext;
}