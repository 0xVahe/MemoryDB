namespace Engine.Serialization.Binary.Checksum;

public enum ChecksumAlgorithm : byte
{
    None = 0,
    Crc32 = 1,
    Custom = 255,
}