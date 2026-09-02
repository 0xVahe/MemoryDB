namespace Engine.Serialization.Binary.Encryption;

public enum EncryptionAlgorithm : byte
{
    None = 0,
    Aes256Gcm = 1,
    Custom = 255,
}