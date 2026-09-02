using System.Text.Json;
using Core;
using Test.Infrastructure;
using Test.Models;

namespace Test.Tests;

public sealed class MemberAccessSerializerTest(string name, string extension, IStorageSerializer serializer) : ITestCase
{
    public string Name { get; } = name + "MemberAccessTest";
    private string Extension { get; } = extension;
    private IStorageSerializer Serializer { get; } = serializer;

    public MemberAccessSerializerTest((string name, string extension, IStorageSerializer serializer) data)
        : this(data.name, data.extension, data.serializer) { }

    public Result<TestExecutionReport> Run(TestConfig config)
    {
        var startedUtc = DateTime.UtcNow;
        var logs = new List<string>();
        var context = new TestContext(config, Name);

        try
        {
            var sourceData = MemberAccessPayload.CreateSample();
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

            var restoredData = Serializer.Deserialize<MemberAccessPayload>(roundtripBytes);
            if (restoredData is null)
                return Fail("Deserialized result is null.", startedUtc, logs);

            string sourceCanonical = JsonSerializer.Serialize(sourceData);
            string restoredCanonical = JsonSerializer.Serialize(restoredData);

            if (!string.Equals(sourceCanonical, restoredCanonical, StringComparison.Ordinal))
                return Fail("Source data and restored data are different.", startedUtc, logs);

            logs.Add($"PublicName: {restoredData.PublicName}");
            logs.Add($"PublicField: {restoredData.PublicField}");
            logs.Add($"Id: {restoredData.Id}");
            logs.Add($"CreatedUtc: {restoredData.CreatedUtc:O}");
            logs.Add($"Nested.Count: {restoredData.Nested.Count}");

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