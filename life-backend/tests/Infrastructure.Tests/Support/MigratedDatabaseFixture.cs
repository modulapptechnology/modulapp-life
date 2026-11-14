using Life.Persistence;
using Life.Support;
using Microsoft.EntityFrameworkCore;

namespace Life.Infrastructure.Tests.Support;

public sealed class MigratedDatabaseFixture : IAsyncLifetime
{
    public const string Project = "Infrastructure";

    private readonly TestDatabase? _database = TestConfiguration.IsConfigured()
        ? new TestDatabase(TestDatabaseOptions.FromConfiguration(TestConfiguration.Build(), Project))
        : null;

    public LifeDbContext CreateContext()
    {
        var connectionString = _database?.ConnectionString
            ?? throw new InvalidOperationException("No test database is configured.");

        return TestLifeDbContext.Create(connectionString);
    }

    public async Task InitializeAsync()
    {
        if (_database is null)
        {
            return;
        }

        await _database.PrepareEmptyAsync();
        await using var context = CreateContext();
        await context.Database.MigrateAsync();
    }

    public Task DisposeAsync()
    {
        return _database?.CleanAsync() ?? Task.CompletedTask;
    }
}
