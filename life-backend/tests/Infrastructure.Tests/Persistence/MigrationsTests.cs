using Infrastructure.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Support;

namespace Infrastructure.Tests.Persistence;

public class MigrationsTests : IAsyncLifetime
{
    private readonly TestDatabase? _database = TestConfiguration.IsConfigured()
        ? new TestDatabase(
            TestDatabaseOptions.FromConfiguration(TestConfiguration.Build(), "InfrastructureMigrations"))
        : null;

    public Task InitializeAsync()
    {
        return _database?.PrepareEmptyAsync() ?? Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        return _database?.CleanAsync() ?? Task.CompletedTask;
    }

    [RequiresTestDatabaseFact]
    public async Task MigrateAsync_OnAnEmptyDatabase_AppliesEveryMigrationInOrder()
    {
        await using var context = CreateContext();

        await context.Database.MigrateAsync();
        var applied = await context.Database.GetAppliedMigrationsAsync();

        Assert.Equal(context.Database.GetMigrations(), applied);
    }

    [RequiresTestDatabaseFact]
    public async Task MigrateAsync_OnAnEmptyDatabase_LeavesNoPendingMigrationsAndMatchesTheModel()
    {
        await using var context = CreateContext();

        await context.Database.MigrateAsync();
        var pending = await context.Database.GetPendingMigrationsAsync();

        Assert.Equal((0, false), (pending.Count(), context.Database.HasPendingModelChanges()));
    }

    [RequiresTestDatabaseFact]
    public async Task MigrateAsync_OnAnEmptyDatabase_CreatesTheUserTables()
    {
        await using var context = CreateContext();

        await context.Database.MigrateAsync();
        var tables = await context.Database
            .SqlQuery<string>($"SELECT TABLE_NAME AS [Value] FROM INFORMATION_SCHEMA.TABLES")
            .ToListAsync();

        var businessTables = tables.Where(name => name != "__EFMigrationsHistory").Order();

        Assert.Equal(
            [
                "EmailVerificationTokens",
                "ExternalAccounts",
                "LoginAttempts",
                "RefreshTokens",
                "Users",
                "WorkspaceInvitations",
                "WorkspaceMembers",
                "Workspaces",
            ],
            businessTables);
    }

    private LifeDbContext CreateContext()
    {
        return TestLifeDbContext.Create(_database!.ConnectionString);
    }
}
