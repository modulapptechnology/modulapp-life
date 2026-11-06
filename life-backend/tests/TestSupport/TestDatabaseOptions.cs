using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace TestSupport;

public sealed partial class TestDatabaseOptions
{
    public const string ConnectionStringKey = "ConnectionStrings:Test";
    public const string DatabaseNameKey = "TestDatabase:Name";
    public const string DefaultDatabaseName = "life_test";

    public TestDatabaseOptions(string? serverConnectionString, string databaseName = DefaultDatabaseName)
    {
        if (string.IsNullOrWhiteSpace(serverConnectionString))
        {
            throw new InvalidOperationException(
                $"Falta la cadena de conexión de pruebas '{ConnectionStringKey}'. " +
                "Defínala con user-secrets (dotnet user-secrets set --project TestSupport) o con la variable ConnectionStrings__Test.");
        }

        if (!NombreValido().IsMatch(databaseName))
        {
            throw new InvalidOperationException(
                $"El nombre de base '{databaseName}' no es válido para pruebas: debe terminar en '_test'.");
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

    public static TestDatabaseOptions FromConfiguration(IConfiguration configuration)
    {
        var nombre = configuration[DatabaseNameKey];
        return string.IsNullOrWhiteSpace(nombre)
            ? new TestDatabaseOptions(configuration[ConnectionStringKey])
            : new TestDatabaseOptions(configuration[ConnectionStringKey], nombre);
    }

    [GeneratedRegex("^[A-Za-z][A-Za-z0-9]*_test$")]
    private static partial Regex NombreValido();
}
