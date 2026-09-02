namespace Engine.Serialization.Binary.Exceptions;

public sealed class BinaryFormatException(string message) : BinarySerializerException(message);
