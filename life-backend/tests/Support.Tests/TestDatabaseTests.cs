using Microsoft.Data.SqlClient;
using Support.Seed;

namespace Support.Tests;

public class TestDatabaseTests
{
    private const string _supportProject = "Support";
    private const string _secondProject = "SupportSecond";

    [RequiresTestDatabaseFact]
    public async Task PrepareAsync_CalledTwice_LeavesOnlyTheSeedData()
    {
        var database = CreateDatabase(_supportProject);
        await database.PrepareAsync();
        await database.PrepareAsync();

        var users = await CountAsync(database.ConnectionString, "dbo.Users");
        await database.CleanAsync();

        Assert.Equal(SeedData.Users.Count, users);
    }

    [RequiresTestDatabaseFact]
    public async Task PrepareEmptyAsync_AfterPrepare_LeavesADatabaseWithoutTablesOrSeedData()
    {
        var database = CreateDatabase(_supportProject);
        await database.PrepareAsync();

        await database.PrepareEmptyAsync();

        var tables = await CountAsync(database.ConnectionString, "INFORMATION_SCHEMA.TABLES");
        await database.CleanAsync();
        Assert.Equal(0, tables);
    }

    [RequiresTestDatabaseFact]
    public async Task CleanAsync_AfterPrepare_DropsTheDatabase()
    {
        var database = CreateDatabase(_supportProject);
        await database.PrepareAsync();

        await database.CleanAsync();

        Assert.False(await database.ExistsAsync());
    }

    [RequiresTestDatabaseFact]
    public async Task CleanAsync_OnOneProjectDatabase_LeavesTheOtherProjectDatabaseUntouched()
    {
        var first = CreateDatabase(_supportProject);
        var second = CreateDatabase(_secondProject);
        await first.PrepareAsync();
        await second.PrepareAsync();

        await first.CleanAsync();
        var users = await CountAsync(second.ConnectionString, "dbo.Users");
        await second.CleanAsync();

        Assert.Equal(SeedData.Users.Count, users);
    }

    private static TestDatabase CreateDatabase(string project)
    {
        var server = TestConfiguration.Build()[TestDatabaseOptions.ConnectionStringKey];
        return new TestDatabase(TestDatabaseOptions.ForProject(server, project));
    }

    private static async Task<int> CountAsync(string connectionString, string table)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand($"SELECT COUNT(*) FROM {table}", connection);
        return (int)(await command.ExecuteScalarAsync())!;
    }
}
