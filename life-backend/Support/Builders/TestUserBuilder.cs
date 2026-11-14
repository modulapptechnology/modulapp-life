namespace Life.Support.Builders;

public sealed record TestUser(Guid Id, string Email, string Name, DateTime CreatedAt);

public sealed class TestUserBuilder
{
    private static int _counter;

    private Guid _id = Guid.NewGuid();
    private string? _email;
    private string? _name;
    private readonly DateTime _createdAt = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public TestUserBuilder WithId(Guid id)
    {
        _id = id;

        return this;
    }

    public TestUserBuilder WithEmail(string email)
    {
        _email = email;

        return this;
    }

    public TestUserBuilder WithName(string name)
    {
        _name = name;

        return this;
    }

    public TestUser Build()
    {
        var number = Interlocked.Increment(ref _counter);

        return new TestUser(
            _id,
            _email ?? $"user{number}@life.test",
            _name ?? $"User {number}",
            _createdAt);
    }
}
