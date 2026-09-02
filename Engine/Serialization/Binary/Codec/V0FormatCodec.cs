using System.Text;

namespace Engine.Serialization.Binary.Codec;

internal sealed class V0FormatCodec: IFormatCodec
{
    public int Version => 0;

    public void Serialize<T>(Stream destination, T data) where T : class
    {
        ArgumentNullException.ThrowIfNull(destination);
        using var writer = new BinaryWriter(destination, Encoding.UTF8, leaveOpen: true);
        var payloadWriter = new BinaryPayloadWriter(writer);
        payloadWriter.Serialize(data);
        writer.Flush();
    }

    public T? Deserialize<T>(Stream source) where T : class
    {
        ArgumentNullException.ThrowIfNull(source);
        using var reader = new BinaryReader(source, Encoding.UTF8, leaveOpen: true);
        var payloadReader = new BinaryPayloadReader(reader);
        return payloadReader.Deserialize<T>();
    }
}