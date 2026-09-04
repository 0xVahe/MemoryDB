using Engine.Serialization.Binary.Checksum;
using Engine.Serialization.Binary.Compression;
using Engine.Serialization.Binary.Encryption;

namespace Engine.Serialization.Binary.Metadata;

public readonly record struct BinaryHeaderInfo(
    int FormatVersion,
    CompressionAlgorithm Compression,
    string? CustomCompressionName,
    ChecksumAlgorithm ChecksumAlgorithm,
    string? CustomChecksumName,
    EncryptionAlgorithm Encryption,
    string? CustomEncryptionName,
    string? KeyId);