using Core;
using Engine.Serialization;
using Engine.Serialization.Binary;
using Engine.Serialization.Binary.Checksum;
using Engine.Serialization.Binary.Compression;
using Engine.Serialization.Binary.Encryption;

namespace Test.Tests;

public static class Serializers
{
    public static (string name, string extension, IStorageSerializer serializer) V0Binary
        => ("BinarySerializer", ".bin", new BinarySerializerStrategy(new BinarySerializer(writeVersion: 0)));
    public static (string name, string extension, IStorageSerializer serializer) DefaultBinary
        => ("BinarySerializer", ".bin", new BinarySerializerStrategy());
    
    public static (string name, string extension, IStorageSerializer serializer) Binary(
        ICompressionAlgorithm? compressionAlgorithm = null,
        IChecksumAlgorithm? checksumAlgorithm = null,
        IEncryptionAlgorithm? encryptionAlgorithm = null,
        IKeyResolver? keyResolver = null,
        string? defaultKeyId = null)
    {
        var compressor = new Compressor(compressionAlgorithm ?? new NoCompression());
        var checksumCalculator = new ChecksumCalculator(checksumAlgorithm ?? new NoChecksum());
        
        Encryptor encryptor;
        if (encryptionAlgorithm is null || keyResolver is null || string.IsNullOrWhiteSpace(defaultKeyId))
        {
            encryptor = Encryptor.None();
        }
        else
        {
            encryptor = new Encryptor(encryptionAlgorithm, keyResolver, defaultKeyId);
        }
        
        var binarySerializer = new BinarySerializer(compressor, checksumCalculator, encryptor);
        var serializer = new BinarySerializerStrategy(binarySerializer);
    
        return ("BinarySerializer", ".bin", serializer);
    }
    
    public static (string name, string extension, IStorageSerializer serializer) Json
        => ("JsonSerializer", ".json", new JsonSerializerStrategy());
    
    public static (string name, string extension, IStorageSerializer serializer) Xml
        => ("XmlSerializer", ".xml", new XmlSerializerStrategy());
}