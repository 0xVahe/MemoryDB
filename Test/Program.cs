using Engine.Serialization.Binary.Compression;
using Test.Infrastructure;
using Test.Tests;

namespace Test;

class Program
{
    static void Main(string[] args)
    {
        var config = TestConfig.CreateDefault();

        var runner = new TestRunner(config)
            .Add(new SerializerTest(Serializers.DefaultBinary))
            .Add(new SerializerTest(Serializers.Binary(new Brotli())))
            .Add(new SerializerTest(Serializers.Binary(new Deflate())))
            .Add(new SerializerTest(Serializers.V0Binary))
            .Add(new SerializerTest(Serializers.Json))
            .Add(new SerializerTest(Serializers.Xml));
            // .Add(new DictionarySerializerTest(Serializers.DefaultBinary))
            // .Add(new MemberAccessSerializerTest(Serializers.DefaultBinary))
            // .Add(new PolymorphismSerializerTest(Serializers.DefaultBinary))
            // .Add(new PolymorphismForbiddenTypeTest(Serializers.DefaultBinary));

        var results = runner.RunAll();

        foreach (var result in results)
        {
            if (result.IsSuccess && result.Value is not null)
            {
                Console.WriteLine($"[PASS] {result.Value.TestName}");
                foreach (var log in result.Value.Logs) Console.WriteLine($"  {log}");
            }
            else
            {
                Console.WriteLine("[FAIL]");
                Console.WriteLine($"  {result.Error}");
                foreach (var log in result.Logs) Console.WriteLine($"  {log}");
            }
        }

        int passed = results.Count(x => x.IsSuccess);
        int failed = results.Count - passed;

        Console.WriteLine();
        Console.WriteLine($"Total: {results.Count}, Passed: {passed}, Failed: {failed}");
    }
}