namespace Support.Builders;

public sealed record TestWorkspace(Guid Id, string Name, Guid OwnerId, DateTime CreatedAt);

public sealed class TestWorkspaceBuilder(Guid ownerId)
{
    private static int _counter;

    private Guid _id = Guid.NewGuid();
    private string? _name;

    public TestWorkspaceBuilder WithId(Guid id)
    {
        _id = id;

        return this;
    }

    public TestWorkspaceBuilder WithName(string name)
    {
        _name = name;

        return this;
    }

    public TestWorkspace Build()
    {
        var number = Interlocked.Increment(ref _counter);

        return new TestWorkspace(
            _id,
            _name ?? $"Workspace {number}",
            ownerId,
            new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    }
}
