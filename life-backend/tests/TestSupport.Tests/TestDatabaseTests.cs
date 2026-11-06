using Microsoft.Data.SqlClient;
using TestSupport.Seed;

namespace TestSupport.Tests;

public class TestDatabaseTests
{
    [RequiresTestDatabaseFact]
    public async Task PrepareAsync_CalledTwice_LeavesOnlyTheSeedData()
    {
        var database = CreateIsolatedDatabase();
        await database.PrepareAsync();
        await database.PrepareAsync();

        var users = await CountAsync(database.ConnectionString, "dbo.Users");
        await database.CleanAsync();

        Assert.Equal(SeedData.Users.Count, users);
    }

    [RequiresTestDatabaseFact]
    public async Task CleanAsync_AfterPrepare_DropsTheDatabase()
    {
        var database = CreateIsolatedDatabase();
        await database.PrepareAsync();

        await database.CleanAsync();

        await Assert.ThrowsAsync<SqlException>(() => CountAsync(database.ConnectionString, "dbo.Users"));
    }

    private static TestDatabase CreateIsolatedDatabase()
    {
        var server = TestConfiguration.Build()[TestDatabaseOptions.ConnectionStringKey];
        return new TestDatabase(new TestDatabaseOptions(server, "life_support_test"));
    }

    private static async Task<int> CountAsync(string connectionString, string table)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand($"SELECT COUNT(*) FROM {table}", connection);
        return (int)(await command.ExecuteScalarAsync())!;
    }
}
