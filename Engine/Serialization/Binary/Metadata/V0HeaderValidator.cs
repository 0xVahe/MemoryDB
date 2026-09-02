using Engine.Serialization.Binary.Exceptions;

namespace Engine.Serialization.Binary.Metadata;

internal sealed class V0HeaderValidator : IHeaderValidator
{
    public int Version => 0;

    public void Validate(object header)
    {
        if (header is not BinaryFormatHeaderV0 h)
            throw new BinaryFormatValidationException("Invalid V0 header object.");

        if (h.FormatVersion != 0)
            throw new BinaryFormatValidationException($"V0 header version mismatch: {h.FormatVersion}.");

        if (h.PayloadLength < 0)
            throw new BinaryFormatValidationException($"V0 PayloadLength must be >= 0, got {h.PayloadLength}.");
    }
}