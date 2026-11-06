using Xunit;

namespace TestSupport;

public sealed class TestDatabaseFixture : IAsyncLifetime
{
    private readonly TestDatabase _database = new(TestDatabaseOptions.FromConfiguration(TestConfiguration.Build()));

    public string ConnectionString => _database.ConnectionString;

    public Task InitializeAsync()
    {
        return _database.PrepareAsync();
    }

    public Task DisposeAsync()
    {
        return _database.CleanAsync();
    }
}
