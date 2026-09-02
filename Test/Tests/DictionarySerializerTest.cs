using System.Text.Json;
using Core;
using Test.Infrastructure;
using Test.Models;

namespace Test.Tests;

public sealed class DictionarySerializerTest(string name, string extension, IStorageSerializer serializer) : ITestCase
{
    public string Name { get; } = name + "DictionaryTest";
    private string Extension { get; } = extension;
    private IStorageSerializer Serializer { get; } = serializer;

    public DictionarySerializerTest((string name, string extension, IStorageSerializer serializer) data)
        : this(data.name, data.extension, data.serializer) { }

    public Result<TestExecutionReport> Run(TestConfig config)
    {
        var startedUtc = DateTime.UtcNow;
        var logs = new List<string>();
        var context = new TestContext(config, Name);

        try
        {
            var sourceData = BuildPayload();
            byte[] serializedData = Serializer.Serialize(sourceData);
            logs.Add($"Serialized bytes length: {serializedData.Length}");

            if (serializedData.Length == 0)
                return Fail("Serialized bytes are empty.", startedUtc, logs);

            byte[] roundtripBytes = serializedData;

            if (config.SaveArtifacts)
            {
                string payloadPath = context.ArtifactPath($"serialized{Extension}");
                File.WriteAllBytes(payloadPath, serializedData);
                roundtripBytes = File.ReadAllBytes(payloadPath);
                logs.Add($"Serialized file: {payloadPath}");
            }

            if (!serializedData.SequenceEqual(roundtripBytes))
                return Fail("Serialized bytes and persisted bytes are different.", startedUtc, logs);

            var restoredData = Serializer.Deserialize<DictionaryRoundTripPayload>(roundtripBytes);
            if (restoredData is null)
                return Fail("Deserialized result is null.", startedUtc, logs);

            string sourceCanonical = JsonSerializer.Serialize(sourceData);
            string restoredCanonical = JsonSerializer.Serialize(restoredData);

            if (!string.Equals(sourceCanonical, restoredCanonical, StringComparison.Ordinal))
                return Fail("Source data and restored data are different.", startedUtc, logs);

            logs.Add($"Counters count: {sourceData.Counters.Count}");
            logs.Add($"LastSeenUtc count: {sourceData.LastSeenUtc.Count}");
            logs.Add($"ProductByCode count: {sourceData.ProductByKey.Count}");

            var report = new TestExecutionReport(
                Name,
                startedUtc,
                DateTime.UtcNow,
                true,
                string.Empty,
                logs
            );

            if (config.SaveArtifacts)
            {
                JsonUtil.WriteJsonFile(context.ArtifactPath("source_data.json"), sourceData);
                JsonUtil.WriteJsonFile(context.ArtifactPath("restored_data.json"), restoredData);
                JsonUtil.WriteJsonFile(context.ArtifactPath("result.json"), report);
            }

            return Result<TestExecutionReport>.Success(report, logs);
        }
        catch (Exception ex)
        {
            return Fail($"Unhandled exception: {ex.GetType().Name}: {ex.Message}", startedUtc, logs);
        }
    }

    private static DictionaryRoundTripPayload BuildPayload()
    {
        var products = TestData.GetProducts();

        var counters = new Dictionary<string, int>
        {
            ["total"] = products.Count,
            ["with_name"] = products.Count(p => !string.IsNullOrWhiteSpace(p.Name)),
            ["without_name"] = products.Count(p => string.IsNullOrWhiteSpace(p.Name))
        };

        var lastSeen = new Dictionary<Guid, DateTime?>
        {
            [Guid.Parse("11111111-1111-1111-1111-111111111111")] = DateTime.UtcNow,
            [Guid.Parse("22222222-2222-2222-2222-222222222222")] = null
        };

        var productByKey = products
            .Select((p, i) => new { Key = $"item_{i}", Value = p })
            .ToDictionary(x => x.Key, x => x.Value);

        return new DictionaryRoundTripPayload
        {
            Counters = counters,
            LastSeenUtc = lastSeen,
            ProductByKey = productByKey
        };
    }

    private Result<TestExecutionReport> Fail(string error, DateTime startedUtc, List<string> logs)
    {
        logs.Add($"Failure: {error}");

        var report = new TestExecutionReport(
            Name,
            startedUtc,
            DateTime.UtcNow,
            false,
            error,
            logs
        );

        return Result<TestExecutionReport>.Failure(error, logs, report);
    }
}