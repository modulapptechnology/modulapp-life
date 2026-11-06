using System.Reflection;
using Microsoft.Data.SqlClient;
using TestSupport.Builders;
using TestSupport.Seed;

namespace TestSupport;

public sealed class TestDatabase
{
    private readonly TestDatabaseOptions _options;

    public TestDatabase(TestDatabaseOptions options)
    {
        _options = options;
    }

    public string ConnectionString => _options.ConnectionString;

    public async Task PrepareAsync()
    {
        await ExecuteAsync(_options.AdminConnectionString, $"IF DB_ID(N'{_options.DatabaseName}') IS NULL CREATE DATABASE [{_options.DatabaseName}];");

        await using var connection = new SqlConnection(_options.ConnectionString);
        await connection.OpenAsync();

        await ExecuteAsync(connection, "DROP TABLE IF EXISTS dbo.Workspaces; DROP TABLE IF EXISTS dbo.Users;");
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
            _options.AdminConnectionString,
            $"IF DB_ID(N'{_options.DatabaseName}') IS NOT NULL BEGIN " +
            $"ALTER DATABASE [{_options.DatabaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; " +
            $"DROP DATABASE [{_options.DatabaseName}]; END");
    }

    public static async Task InsertAsync(SqlConnection connection, TestUser user)
    {
        await using var command = new SqlCommand(
            "INSERT INTO dbo.Users (Id, Email, Name, CreatedAt) VALUES (@Id, @Email, @Name, @CreatedAt)", connection);
        command.Parameters.AddWithValue("@Id", user.Id);
        command.Parameters.AddWithValue("@Email", user.Email);
        command.Parameters.AddWithValue("@Name", user.Name);
        command.Parameters.AddWithValue("@CreatedAt", user.CreatedAt);
        await command.ExecuteNonQueryAsync();
    }

    public static async Task InsertAsync(SqlConnection connection, TestWorkspace workspace)
    {
        await using var command = new SqlCommand(
            "INSERT INTO dbo.Workspaces (Id, Name, OwnerId, CreatedAt) VALUES (@Id, @Name, @OwnerId, @CreatedAt)", connection);
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
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("TestSupport.Schema.sql")
            ?? throw new InvalidOperationException("The TestSupport.Schema.sql resource was not found.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
