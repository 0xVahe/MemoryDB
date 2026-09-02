using System.Text.Json;
using Core;
using Test.Infrastructure;
using Test.Models;

namespace Test.Tests;

public sealed class PolymorphismSerializerTest(string name, string extension, IStorageSerializer serializer) : ITestCase
{
    public string Name { get; } = name + "PolymorphismTest";
    private string Extension { get; } = extension;
    private IStorageSerializer Serializer { get; } = serializer;

    public PolymorphismSerializerTest((string name, string extension, IStorageSerializer serializer) data)
        : this(data.name, data.extension, data.serializer) { }

    public Result<TestExecutionReport> Run(TestConfig config)
    {
        var startedUtc = DateTime.UtcNow;
        var logs = new List<string>();
        var context = new TestContext(config, Name);

        try
        {
            var sourceData = new OrderWithPolymorphism
            {
                Id = 1001,
                Payment = new CardPayment
                {
                    Currency = "USD",
                    Last4 = "4242"
                }
            };

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

            var restored = Serializer.Deserialize<OrderWithPolymorphism>(roundtripBytes);
            if (restored is null)
                return Fail("Deserialized result is null.", startedUtc, logs);

            if (restored.Payment is not CardPayment restoredCard)
                return Fail("Restored payment runtime type is not CardPayment.", startedUtc, logs);

            string sourceCanonical = JsonSerializer.Serialize(sourceData);
            string restoredCanonical = JsonSerializer.Serialize(restored);

            if (!string.Equals(sourceCanonical, restoredCanonical, StringComparison.Ordinal))
                return Fail("Source data and restored data are different.", startedUtc, logs);

            logs.Add($"Restored payment type: {restored.Payment.GetType().Name}");
            logs.Add($"Restored card last4: {restoredCard.Last4}");

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
                JsonUtil.WriteJsonFile(context.ArtifactPath("restored_data.json"), restored);
                JsonUtil.WriteJsonFile(context.ArtifactPath("result.json"), report);
            }

            return Result<TestExecutionReport>.Success(report, logs);
        }
        catch (Exception ex)
        {
            return Fail($"Unhandled exception: {ex.GetType().Name}: {ex.Message}", startedUtc, logs);
        }
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