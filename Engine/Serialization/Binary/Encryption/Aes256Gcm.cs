using System.Security.Cryptography;
using Engine.Serialization.Binary.Exceptions;

namespace Engine.Serialization.Binary.Encryption;

public sealed class Aes256Gcm : IEncryptionAlgorithm
{
    private const int KeySizeBytes = 32;
    private const int NonceSizeBytes = 12;
    private const int TagSizeBytes = 16;

    public EncryptionAlgorithm Kind => EncryptionAlgorithm.Aes256Gcm;
    public string? CustomName => null;

    public byte[] Encrypt(byte[] plaintext, byte[] key)
    {
        ArgumentNullException.ThrowIfNull(plaintext);
        ValidateKey(key);

        Span<byte> nonce = stackalloc byte[NonceSizeBytes];
        RandomNumberGenerator.Fill(nonce);

        var ciphertext = new byte[plaintext.Length];
        var tag = new byte[TagSizeBytes];

        using var aes = new AesGcm(key, TagSizeBytes);
        aes.Encrypt(nonce, plaintext, ciphertext, tag);

        var result = new byte[NonceSizeBytes + ciphertext.Length + TagSizeBytes];
        nonce.CopyTo(result);
        ciphertext.CopyTo(result, NonceSizeBytes);
        tag.CopyTo(result, NonceSizeBytes + ciphertext.Length);
        return result;
    }

    public byte[] Decrypt(byte[] ciphertext, byte[] key, int expectedPlaintextLength)
    {
        ArgumentNullException.ThrowIfNull(ciphertext);
        ValidateKey(key);

        if (ciphertext.Length < NonceSizeBytes + TagSizeBytes)
            throw new BinaryFormatException("Ciphertext is too short for AES-GCM.");

        int expectedLength = NonceSizeBytes + expectedPlaintextLength + TagSizeBytes;
        if (ciphertext.Length != expectedLength)
            throw new BinaryFormatException($"Ciphertext length mismatch. Expected {expectedLength}, got {ciphertext.Length}.");

        var nonce = ciphertext.AsSpan(0, NonceSizeBytes);
        var encryptedPayload = ciphertext.AsSpan(NonceSizeBytes, expectedPlaintextLength);
        var tag = ciphertext.AsSpan(NonceSizeBytes + expectedPlaintextLength, TagSizeBytes);

        var plaintext = new byte[expectedPlaintextLength];
        try
        {
            using var aes = new AesGcm(key, TagSizeBytes);
            aes.Decrypt(nonce, encryptedPayload, tag, plaintext);
            return plaintext;
        }
        catch (CryptographicException ex)
        {
            throw new BinaryIntegrityException("Decryption failed: wrong key or tampered payload.", ex);
        }
    }

    private static void ValidateKey(byte[] key)
    {
        ArgumentNullException.ThrowIfNull(key);
        if (key.Length != KeySizeBytes)
            throw new ArgumentException($"Aes256Gcm requires a {KeySizeBytes}-byte (256-bit) key, got {key.Length}.", nameof(key));
    }
}