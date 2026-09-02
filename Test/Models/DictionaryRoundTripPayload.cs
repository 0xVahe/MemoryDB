namespace Test.Models;

public sealed class DictionaryRoundTripPayload
{
    public Dictionary<string, int> Counters { get; init; } = [];
    public Dictionary<Guid, DateTime?> LastSeenUtc { get; init; } = [];
    public Dictionary<string, Product> ProductByKey { get; init; } = [];
}