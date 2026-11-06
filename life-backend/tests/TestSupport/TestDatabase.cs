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
        await EjecutarAsync(_options.AdminConnectionString, $"IF DB_ID(N'{_options.DatabaseName}') IS NULL CREATE DATABASE [{_options.DatabaseName}];");

        await using var connection = new SqlConnection(_options.ConnectionString);
        await connection.OpenAsync();

        await EjecutarAsync(connection, "DROP TABLE IF EXISTS dbo.EspaciosTrabajo; DROP TABLE IF EXISTS dbo.Usuarios;");
        await EjecutarAsync(connection, LeerEsquema());

        foreach (var usuario in DatosSemilla.Usuarios)
        {
            await InsertarAsync(connection, usuario);
        }

        foreach (var espacio in DatosSemilla.Espacios)
        {
            await InsertarAsync(connection, espacio);
        }
    }

    public async Task CleanAsync()
    {
        await EjecutarAsync(
            _options.AdminConnectionString,
            $"IF DB_ID(N'{_options.DatabaseName}') IS NOT NULL BEGIN " +
            $"ALTER DATABASE [{_options.DatabaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; " +
            $"DROP DATABASE [{_options.DatabaseName}]; END");
    }

    public static async Task InsertarAsync(SqlConnection connection, UsuarioPrueba usuario)
    {
        await using var command = new SqlCommand(
            "INSERT INTO dbo.Usuarios (Id, Email, Nombre, CreadoEn) VALUES (@Id, @Email, @Nombre, @CreadoEn)", connection);
        command.Parameters.AddWithValue("@Id", usuario.Id);
        command.Parameters.AddWithValue("@Email", usuario.Email);
        command.Parameters.AddWithValue("@Nombre", usuario.Nombre);
        command.Parameters.AddWithValue("@CreadoEn", usuario.CreadoEn);
        await command.ExecuteNonQueryAsync();
    }

    public static async Task InsertarAsync(SqlConnection connection, EspacioTrabajoPrueba espacio)
    {
        await using var command = new SqlCommand(
            "INSERT INTO dbo.EspaciosTrabajo (Id, Nombre, PropietarioId, CreadoEn) VALUES (@Id, @Nombre, @PropietarioId, @CreadoEn)", connection);
        command.Parameters.AddWithValue("@Id", espacio.Id);
        command.Parameters.AddWithValue("@Nombre", espacio.Nombre);
        command.Parameters.AddWithValue("@PropietarioId", espacio.PropietarioId);
        command.Parameters.AddWithValue("@CreadoEn", espacio.CreadoEn);
        await command.ExecuteNonQueryAsync();
    }

    private static async Task EjecutarAsync(string connectionString, string sql)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        await EjecutarAsync(connection, sql);
    }

    private static async Task EjecutarAsync(SqlConnection connection, string sql)
    {
        await using var command = new SqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }

    private static string LeerEsquema()
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("TestSupport.Schema.sql")
            ?? throw new InvalidOperationException("No se encontró el recurso TestSupport.Schema.sql.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
