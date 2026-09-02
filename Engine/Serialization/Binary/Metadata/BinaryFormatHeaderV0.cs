using Engine.Serialization.Binary.Exceptions;
using Engine.Serialization.Binary.Format;

namespace Engine.Serialization.Binary.Metadata;

internal readonly record struct BinaryFormatHeaderV0(
    int FormatVersion,
    int PayloadLength)
{
    public void WriteTo(BinaryWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);

        writer.Write(BinaryFormatConstants.Magic);
        writer.Write(FormatVersion);
        writer.Write(PayloadLength);
    }

    public static BinaryFormatHeaderV0 ReadFrom(BinaryReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);

        int magic = reader.ReadInt32();
        if (magic != BinaryFormatConstants.Magic)
            throw new BinaryFormatValidationException("Not a recognized BinarySerializer stream (magic number mismatch).");

        int version = reader.ReadInt32();
        int payloadLength = reader.ReadInt32();

        if (payloadLength < 0)
            throw new BinaryFormatValidationException($"PayloadLength must be >= 0, got {payloadLength}.");

        return new BinaryFormatHeaderV0(version, payloadLength);
    }
}