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
        => ("BinarySerializer",
            ".bin", 
            new BinarySerializerStrategy(
                new BinarySerializer(
                    new BinarySerializerOptions() with { WriteVersion = 0, AllowV0Fallback = true }
                )
            ));
    public static (string name, string extension, IStorageSerializer serializer) DefaultBinary
        => ("BinarySerializer", ".bin", new BinarySerializerStrategy());
    
    public static (string name, string extension, IStorageSerializer serializer) Binary(
        ICompressionAlgorithm? compressionAlgorithm = null,
        IChecksumAlgorithm? checksumAlgorithm = null,
        IEncryptionAlgorithm? encryptionAlgorithm = null,
        byte[]? key = null, 
        string? defaultKeyId = null,
        int writeVersion = 1,
        bool allowV0Fallback = false)
    {
        var compressor = new Compressor(compressionAlgorithm ?? new NoCompression());
        var checksumCalculator = new ChecksumCalculator(checksumAlgorithm ?? new NoChecksum());

        key ??= Secrets.Key;
        defaultKeyId ??= Secrets.KeyId;
        var encryptor =  new Encryptor(encryptionAlgorithm ?? new NoEncryption(), key, defaultKeyId);

        var options = new BinarySerializerOptions
        {
            Compressor = compressor,
            Checksum = checksumCalculator,
            Encryptor = encryptor,
            WriteVersion = writeVersion,
            AllowV0Fallback = allowV0Fallback
        };

        var binarySerializer = new BinarySerializer(options);
        var serializer = new BinarySerializerStrategy(binarySerializer);

        return ("BinarySerializer", ".bin", serializer);
    }
    
    public static (string name, string extension, IStorageSerializer serializer) Json
        => ("JsonSerializer", ".json", new JsonSerializerStrategy());
    
    public static (string name, string extension, IStorageSerializer serializer) Xml
        => ("XmlSerializer", ".xml", new XmlSerializerStrategy());
}