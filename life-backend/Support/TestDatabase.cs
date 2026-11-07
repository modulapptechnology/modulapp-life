using System.Reflection;
using Microsoft.Data.SqlClient;
using Support.Builders;
using Support.Seed;

namespace Support;

public sealed class TestDatabase(TestDatabaseOptions options)
{
    public string ConnectionString => options.ConnectionString;

    public async Task<bool> ExistsAsync()
    {
        await using var connection = new SqlConnection(options.AdminConnectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand("SELECT DB_ID(@Name)", connection);
        command.Parameters.AddWithValue("@Name", options.DatabaseName);

        return await command.ExecuteScalarAsync() is int;
    }

    public async Task PrepareAsync()
    {
        await ExecuteAsync(
            options.AdminConnectionString,
            $"IF DB_ID(N'{options.DatabaseName}') IS NULL CREATE DATABASE [{options.DatabaseName}];");

        await using var connection = new SqlConnection(options.ConnectionString);
        await connection.OpenAsync();

        await ExecuteAsync(
            connection,
            "DROP TABLE IF EXISTS dbo.Workspaces; DROP TABLE IF EXISTS dbo.Users;");
        await ExecuteAsync(connection, ReadSchema());

        foreach (var user in SeedData.Users)
        {
            await InsertAsync(connection, user);
        }

        foreach (var workspace in SeedData.Workspaces)
        {
            await InsertAsync(connection, workspace);
        }
    }

    public async Task CleanAsync()
    {
        await ExecuteAsync(
            options.AdminConnectionString,
            $"IF DB_ID(N'{options.DatabaseName}') IS NOT NULL BEGIN "
            + $"ALTER DATABASE [{options.DatabaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; "
            + $"DROP DATABASE [{options.DatabaseName}]; END");
    }

    public static async Task InsertAsync(SqlConnection connection, TestUser user)
    {
        await using var command = new SqlCommand(
            "INSERT INTO dbo.Users (Id, Email, Name, CreatedAt) VALUES (@Id, @Email, @Name, @CreatedAt)",
            connection);
        command.Parameters.AddWithValue("@Id", user.Id);
        command.Parameters.AddWithValue("@Email", user.Email);
        command.Parameters.AddWithValue("@Name", user.Name);
        command.Parameters.AddWithValue("@CreatedAt", user.CreatedAt);
        await command.ExecuteNonQueryAsync();
    }

    public static async Task InsertAsync(SqlConnection connection, TestWorkspace workspace)
    {
        await using var command = new SqlCommand(
            "INSERT INTO dbo.Workspaces (Id, Name, OwnerId, CreatedAt) "
            + "VALUES (@Id, @Name, @OwnerId, @CreatedAt)",
            connection);
        command.Parameters.AddWithValue("@Id", workspace.Id);
        command.Parameters.AddWithValue("@Name", workspace.Name);
        command.Parameters.AddWithValue("@OwnerId", workspace.OwnerId);
        command.Parameters.AddWithValue("@CreatedAt", workspace.CreatedAt);
        await command.ExecuteNonQueryAsync();
    }

    private static async Task ExecuteAsync(string connectionString, string sql)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        await ExecuteAsync(connection, sql);
    }

    private static async Task ExecuteAsync(SqlConnection connection, string sql)
    {
        await using var command = new SqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }

    private static string ReadSchema()
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Support.Schema.sql")
            ?? throw new InvalidOperationException("The Support.Schema.sql resource was not found.");
        using var reader = new StreamReader(stream);

        return reader.ReadToEnd();
    }
}
