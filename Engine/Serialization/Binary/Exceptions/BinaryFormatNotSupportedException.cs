namespace Engine.Serialization.Binary.Exceptions;

public sealed class BinaryFormatNotSupportedException(string message) : BinarySerializerException(message);
