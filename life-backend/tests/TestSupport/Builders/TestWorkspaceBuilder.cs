namespace TestSupport.Builders;

public sealed record TestWorkspace(Guid Id, string Name, Guid OwnerId, DateTime CreatedAt);

public sealed class TestWorkspaceBuilder
{
    private static int _counter;

    private readonly Guid _ownerId;
    private Guid _id = Guid.NewGuid();
    private string? _name;

    public TestWorkspaceBuilder(Guid ownerId)
    {
        _ownerId = ownerId;
    }

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
            _ownerId,
            new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    }
}
