using Engine.Serialization.Binary.Checksum;
using Engine.Serialization.Binary.Compression;
using Engine.Serialization.Binary.Encryption;
using Engine.Serialization.Binary.Exceptions;

namespace Engine.Serialization.Binary.Metadata;

internal sealed class V1HeaderValidator : IHeaderValidator
{
    public int Version => 1;

    public void Validate(object header)
    {
        if (header is not BinaryFormatHeaderV1 h)
            throw new BinaryFormatValidationException("Invalid V1 header object.");

        if (h.FormatVersion != 1)
            throw new BinaryFormatValidationException($"V1 header version mismatch: {h.FormatVersion}.");

        if (!Enum.IsDefined(typeof(CompressionAlgorithm), h.Compression))
            throw new BinaryFormatValidationException($"Unknown compression kind: {h.Compression}.");

        if (!Enum.IsDefined(typeof(ChecksumAlgorithm), h.ChecksumAlgorithm))
            throw new BinaryFormatValidationException($"Unknown checksum kind: {h.ChecksumAlgorithm}.");

        if (!Enum.IsDefined(typeof(EncryptionAlgorithm), h.Encryption))
            throw new BinaryFormatValidationException($"Unknown encryption kind: {h.Encryption}.");

        if (h.UncompressedLength < 0 || h.CompressedLength < 0 || h.OnDiskLength < 0)
            throw new BinaryFormatValidationException("V1 lengths must be non-negative.");

        if (h.Encryption == EncryptionAlgorithm.None)
        {
            if (!string.IsNullOrWhiteSpace(h.KeyId))
                throw new BinaryFormatValidationException("KeyId must be empty when encryption is None.");
            if (h.OnDiskLength != h.CompressedLength)
                throw new BinaryFormatValidationException("OnDiskLength must equal CompressedLength when encryption is None.");
        }
        else
        {
            if (string.IsNullOrWhiteSpace(h.KeyId))
                throw new BinaryFormatValidationException("Encrypted V1 payload requires non-empty KeyId.");
        }

        int expectedChecksumLength = h.ChecksumAlgorithm switch
        {
            ChecksumAlgorithm.None => 0,
            ChecksumAlgorithm.Crc32 => 4,
            _ => throw new BinaryFormatValidationException($"Unsupported checksum kind: {h.ChecksumAlgorithm}")
        };

        if (h.Checksum.Length != expectedChecksumLength)
            throw new BinaryFormatValidationException(
                $"Checksum length mismatch for {h.ChecksumAlgorithm}. Expected {expectedChecksumLength}, got {h.Checksum.Length}.");
    }
}