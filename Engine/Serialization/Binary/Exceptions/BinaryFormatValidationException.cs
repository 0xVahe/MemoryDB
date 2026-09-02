namespace Engine.Serialization.Binary.Exceptions;

public sealed class BinaryFormatValidationException : Exception
{
    public BinaryFormatValidationException(string message) : base(message) { }

    public BinaryFormatValidationException(string message, Exception innerException)
        : base(message, innerException) { }
}