using Xunit;

namespace Support;

public class TestDatabaseFixture(string project) : IAsyncLifetime
{
    private readonly TestDatabase _database = new(
        TestDatabaseOptions.FromConfiguration(TestConfiguration.Build(), project));

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
