using Engine.Serialization.Binary.Checksum;
using Engine.Serialization.Binary.Compression;
using Engine.Serialization.Binary.Encryption;
using Engine.Serialization.Binary.Exceptions;
using Engine.Serialization.Binary.Format;

namespace Engine.Serialization.Binary.Metadata;

internal readonly record struct BinaryFormatHeaderV1(
    int FormatVersion,
    CompressionAlgorithm Compression,
    string? CustomCompressionName,
    ChecksumAlgorithm ChecksumAlgorithm,
    string? CustomChecksumName,
    EncryptionAlgorithm Encryption,
    string? CustomEncryptionName,
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
        WriteOptionalString(writer, Compression == CompressionAlgorithm.Custom ? CustomCompressionName : null);

        writer.Write((byte)ChecksumAlgorithm);
        WriteOptionalString(writer, ChecksumAlgorithm == ChecksumAlgorithm.Custom ? CustomChecksumName : null);

        writer.Write((byte)Encryption);
        WriteOptionalString(writer, Encryption == EncryptionAlgorithm.Custom ? CustomEncryptionName : null);

        WriteOptionalString(writer, KeyId);

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
            throw new BinaryFormatException("Not a recognized BinarySerializer stream (magic number mismatch).");

        int formatVersion = reader.ReadInt32();

        var compression = (CompressionAlgorithm)reader.ReadByte();
        if (!Enum.IsDefined(compression))
            throw new BinaryFormatNotSupportedException($"Unknown compression algorithm: {compression}.");
        string? customCompressionName = ReadOptionalString(reader);

        var checksumAlgorithm = (ChecksumAlgorithm)reader.ReadByte();
        if (!Enum.IsDefined(checksumAlgorithm))
            throw new BinaryFormatNotSupportedException($"Unknown checksum algorithm: {checksumAlgorithm}.");
        string? customChecksumName = ReadOptionalString(reader);

        var encryption = (EncryptionAlgorithm)reader.ReadByte();
        if (!Enum.IsDefined(encryption))
            throw new BinaryFormatNotSupportedException($"Unknown encryption algorithm: {encryption}.");
        string? customEncryptionName = ReadOptionalString(reader);

        string? keyId = ReadOptionalString(reader);

        int uncompressedLength = reader.ReadInt32();
        int compressedLength = reader.ReadInt32();
        int onDiskLength = reader.ReadInt32();

        if (uncompressedLength < 0 || compressedLength < 0 || onDiskLength < 0)
            throw new BinaryFormatException("V1 lengths must be non-negative.");

        if (encryption == EncryptionAlgorithm.None && onDiskLength != compressedLength)
            throw new BinaryFormatException("OnDiskLength must equal CompressedLength when encryption is None.");

        byte checksumLength = reader.ReadByte();
        byte[] checksum = reader.ReadBytes(checksumLength);
        if (checksum.Length != checksumLength)
            throw new BinaryFormatException($"Checksum bytes ended early. Expected {checksumLength}, got {checksum.Length}.");

        return new BinaryFormatHeaderV1(
            formatVersion,
            compression, customCompressionName,
            checksumAlgorithm, customChecksumName,
            encryption, customEncryptionName,
            keyId,
            uncompressedLength, compressedLength, onDiskLength,
            checksum);
    }

    private static void WriteOptionalString(BinaryWriter writer, string? value)
    {
        bool has = !string.IsNullOrEmpty(value);
        writer.Write(has);
        if (has) writer.Write(value!);
    }

    private static string? ReadOptionalString(BinaryReader reader) => reader.ReadBoolean() ? reader.ReadString() : null;
}