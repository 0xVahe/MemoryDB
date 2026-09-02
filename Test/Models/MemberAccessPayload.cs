namespace Test.Models;

public sealed class MemberAccessPayload
{
    public string PublicName { get; set; } = string.Empty;

    public int PublicField;

    public Guid Id { get; private set; }

    public DateTime CreatedUtc { get; init; }

    public NestedMemberPayload Nested { get; set; } = new();

    public MemberAccessPayload() { }

    public MemberAccessPayload(Guid id)
    {
        Id = id;
    }

    public static MemberAccessPayload CreateSample()
    {
        return new MemberAccessPayload(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"))
        {
            PublicName = "member-access",
            PublicField = 42,
            CreatedUtc = new DateTime(2026, 1, 15, 12, 30, 0, DateTimeKind.Utc),
            Nested = new NestedMemberPayload
            {
                Count = 7,
                Note = "nested"
            }
        };
    }
}

public sealed class NestedMemberPayload
{
    public int Count { get; set; }
    public string Note { get; set; } = string.Empty;
}