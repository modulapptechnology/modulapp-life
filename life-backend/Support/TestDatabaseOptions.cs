using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Support;

public sealed partial class TestDatabaseOptions
{
    public const string ConnectionStringKey = "ConnectionStrings:Test";

    public TestDatabaseOptions(string? serverConnectionString, string databaseName)
    {
        if (string.IsNullOrWhiteSpace(serverConnectionString))
        {
            throw new InvalidOperationException(
                $"Missing the test connection string '{ConnectionStringKey}'. "
                + "Set it with user-secrets (dotnet user-secrets set --project Support) "
                + "or with the ConnectionStrings__Test environment variable.");
        }

        if (!ValidName().IsMatch(databaseName))
        {
            throw new InvalidOperationException(
                $"The database name '{databaseName}' is not valid for tests: it must end in 'Test'.");
        }

        var builder = new SqlConnectionStringBuilder(serverConnectionString) { InitialCatalog = "master" };
        AdminConnectionString = builder.ConnectionString;

        builder.InitialCatalog = databaseName;
        ConnectionString = builder.ConnectionString;
        DatabaseName = databaseName;
    }

    public string DatabaseName { get; }

    public string AdminConnectionString { get; }

    public string ConnectionString { get; }

    public static string NameForProject(string project)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(project);

        return $"ModulLife{project}Test";
    }

    public static TestDatabaseOptions ForProject(string? serverConnectionString, string project)
    {
        return new TestDatabaseOptions(serverConnectionString, NameForProject(project));
    }

    public static TestDatabaseOptions FromConfiguration(IConfiguration configuration, string project)
    {
        return ForProject(configuration[ConnectionStringKey], project);
    }

    [GeneratedRegex("^[A-Za-z][A-Za-z0-9]*Test$")]
    private static partial Regex ValidName();
}
