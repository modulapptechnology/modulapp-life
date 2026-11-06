using Microsoft.Data.SqlClient;
using TestSupport.Seed;

namespace TestSupport.Tests;

public class TestDatabaseTests
{
    [RequiereBaseDePruebasFact]
    public async Task PrepareAsync_LlamadoDosVeces_DejaSoloLosDatosSemilla()
    {
        var database = CrearBaseAislada();
        await database.PrepareAsync();
        await database.PrepareAsync();

        var usuarios = await ContarAsync(database.ConnectionString, "dbo.Usuarios");
        await database.CleanAsync();

        Assert.Equal(DatosSemilla.Usuarios.Count, usuarios);
    }

    [RequiereBaseDePruebasFact]
    public async Task CleanAsync_DespuesDePreparar_EliminaLaBase()
    {
        var database = CrearBaseAislada();
        await database.PrepareAsync();

        await database.CleanAsync();

        await Assert.ThrowsAsync<SqlException>(() => ContarAsync(database.ConnectionString, "dbo.Usuarios"));
    }

    private static TestDatabase CrearBaseAislada()
    {
        var servidor = TestConfiguration.Build()[TestDatabaseOptions.ConnectionStringKey];
        return new TestDatabase(new TestDatabaseOptions(servidor, "life_support_test"));
    }

    private static async Task<int> ContarAsync(string connectionString, string tabla)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand($"SELECT COUNT(*) FROM {tabla}", connection);
        return (int)(await command.ExecuteScalarAsync())!;
    }
}
