using Engine.Serialization.Binary.Checksum;
using Engine.Serialization.Binary.Compression;
using Engine.Serialization.Binary.Encryption;

namespace Engine.Serialization.Binary.Metadata;

public readonly record struct BinaryHeaderInfo(
    int FormatVersion,
    CompressionAlgorithm Compression,
    ChecksumAlgorithm ChecksumAlgorithm,
    EncryptionAlgorithm Encryption,
    string? KeyId);
