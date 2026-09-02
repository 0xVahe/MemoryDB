namespace Engine.Serialization.Binary.Checksum;

public sealed class Crc32 : IChecksumAlgorithm
{
    private const uint Polynomial = 0xEDB88320;
    private static readonly uint[] Table = BuildTable();

    public ChecksumAlgorithm Kind => ChecksumAlgorithm.Crc32;
    public string? CustomName => null;

    public byte[] Compute(ReadOnlySpan<byte> data)
    {
        uint crc = 0xFFFFFFFF;
        foreach (byte b in data)
            crc = Table[(crc ^ b) & 0xFF] ^ (crc >> 8);
        crc ^= 0xFFFFFFFF;

        return BitConverter.GetBytes(crc);
    }

    private static uint[] BuildTable()
    {
        var table = new uint[256];
        for (uint i = 0; i < 256; i++)
        {
            uint c = i;
            for (int bit = 0; bit < 8; bit++)
                c = (c & 1) != 0 ? Polynomial ^ (c >> 1) : c >> 1;
            table[i] = c;
        }
        return table;
    }
}