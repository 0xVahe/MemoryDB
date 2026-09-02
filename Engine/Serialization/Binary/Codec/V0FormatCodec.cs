using System.Text;
using Engine.Serialization.Binary.Metadata;

namespace Engine.Serialization.Binary.Codec;

internal sealed class V0FormatCodec : IFormatCodec
{
    public int Version => 0;

    public void Serialize<T>(Stream destination, T data) where T : class
    {
        ArgumentNullException.ThrowIfNull(destination);

        byte[] rawPayload = SerializePayload(data);

        var header = new BinaryFormatHeaderV0(
            FormatVersion: Version,
            PayloadLength: rawPayload.Length);

        using var writer = new BinaryWriter(destination, Encoding.UTF8, leaveOpen: true);
        header.WriteTo(writer);
        writer.Write(rawPayload);
        writer.Flush();
    }

    public T? Deserialize<T>(Stream source) where T : class
    {
        ArgumentNullException.ThrowIfNull(source);

        using var reader = new BinaryReader(source, Encoding.UTF8, leaveOpen: true);
        var header = BinaryFormatHeaderV0.ReadFrom(reader);

        if (header.FormatVersion != Version)
            throw new InvalidDataException($"Binary format version {header.FormatVersion} is not supported by V0 codec.");

        byte[] rawPayload = reader.ReadBytes(header.PayloadLength);
        if (rawPayload.Length != header.PayloadLength)
            throw new InvalidDataException(
                $"Payload ended early. Expected {header.PayloadLength} bytes, got {rawPayload.Length}.");

        return DeserializePayload<T>(rawPayload);
    }

    private static byte[] SerializePayload<T>(T data) where T : class
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms, Encoding.UTF8, leaveOpen: true);

        new BinaryPayloadWriter(writer).Serialize(data);
        writer.Flush();
        return ms.ToArray();
    }

    private static T? DeserializePayload<T>(byte[] rawPayload) where T : class
    {
        using var ms = new MemoryStream(rawPayload);
        using var reader = new BinaryReader(ms, Encoding.UTF8, leaveOpen: true);

        return new BinaryPayloadReader(reader).Deserialize<T>();
    }
}