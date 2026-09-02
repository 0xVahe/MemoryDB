using Engine.Serialization.Binary.Checksum;
using Engine.Serialization.Binary.Compression;
using Engine.Serialization.Binary.Encryption;
using Engine.Serialization.Binary.Exceptions;
using Engine.Serialization.Binary.Format;

namespace Engine.Serialization.Binary.Metadata;

internal readonly record struct BinaryFormatHeaderV1(
    int FormatVersion,
    CompressionAlgorithm Compression,
    ChecksumAlgorithm ChecksumAlgorithm,
    EncryptionAlgorithm Encryption,
    string? KeyId,
    int UncompressedLength,
    int CompressedLength,
    int OnDiskLength,
    byte[] Checksum)
{
    public void WriteTo(BinaryWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);

        writer.Write(BinaryFormatConstants.Magic);
        writer.Write(FormatVersion);
        writer.Write((byte)Compression);
        writer.Write((byte)ChecksumAlgorithm);
        writer.Write((byte)Encryption);

        bool hasKeyId = !string.IsNullOrWhiteSpace(KeyId);
        writer.Write(hasKeyId);
        if (hasKeyId) writer.Write(KeyId!);

        writer.Write(UncompressedLength);
        writer.Write(CompressedLength);
        writer.Write(OnDiskLength);

        writer.Write((byte)Checksum.Length);
        if (Checksum.Length > 0) writer.Write(Checksum);
    }

    public static BinaryFormatHeaderV1 ReadFrom(BinaryReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);

        int magic = reader.ReadInt32();
        if (magic != BinaryFormatConstants.Magic)
            throw new BinaryFormatValidationException("Not a recognized BinarySerializer stream (magic number mismatch).");

        int formatVersion = reader.ReadInt32();
        var compression = (CompressionAlgorithm)reader.ReadByte();
        var checksumAlgorithm = (ChecksumAlgorithm)reader.ReadByte();
        var encryption = (EncryptionAlgorithm)reader.ReadByte();

        bool hasKeyId = reader.ReadBoolean();
        string? keyId = hasKeyId ? reader.ReadString() : null;

        int uncompressedLength = reader.ReadInt32();
        int compressedLength = reader.ReadInt32();
        int onDiskLength = reader.ReadInt32();

        byte checksumLength = reader.ReadByte();
        byte[] checksum = reader.ReadBytes(checksumLength);

        if (checksum.Length != checksumLength)
            throw new BinaryFormatValidationException(
                $"Checksum bytes ended early. Expected {checksumLength}, got {checksum.Length}.");

        return new BinaryFormatHeaderV1(
            formatVersion,
            compression,
            checksumAlgorithm,
            encryption,
            keyId,
            uncompressedLength,
            compressedLength,
            onDiskLength,
            checksum);
    }
}