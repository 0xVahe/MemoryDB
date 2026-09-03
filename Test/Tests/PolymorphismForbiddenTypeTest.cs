using Core;
using Engine.Serialization.Binary.Exceptions;
using Test.Infrastructure;
using Test.Models;

namespace Test.Tests;

public sealed class PolymorphismForbiddenTypeTest(string name, string extension, IStorageSerializer serializer) : ITestCase
{
    public string Name { get; } = name + "PolymorphismForbiddenTypeTest";
    private IStorageSerializer Serializer { get; } = serializer;

    public PolymorphismForbiddenTypeTest((string name, string extension, IStorageSerializer serializer) data)
        : this(data.name, data.extension, data.serializer) { }

    public Result<TestExecutionReport> Run(TestConfig config)
    {
        var startedUtc = DateTime.UtcNow;
        var logs = new List<string>();

        try
        {
            var sourceData = new OrderWithPolymorphism
            {
                Id = 2002,
                Payment = new CryptoPayment
                {
                    Currency = "USD",
                    Wallet = "0xABCDEF"
                }
            };

            Serializer.Serialize(sourceData);
            return Fail("Serialization should fail for a forbidden runtime type, but it succeeded.", startedUtc, logs);
        }
        catch (BinaryTypeException ex) 
        {
            logs.Add($"Expected failure captured successfully: {ex.Message}");

            var report = new TestExecutionReport(
                Name,
                startedUtc,
                DateTime.UtcNow,
                true,
                string.Empty,
                logs
            );

            return Result<TestExecutionReport>.Success(report, logs);
        }
        catch (Exception ex)
        {
            return Fail($"Unexpected exception type: {ex.GetType().Name}: {ex.Message}", startedUtc, logs);
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