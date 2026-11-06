using Microsoft.Data.SqlClient;

namespace TestSupport.Tests;

public class TestDatabaseOptionsTests
{
    private const string Servidor = "Server=localhost,1433;User Id=sa;Password=x;TrustServerCertificate=True";

    [Fact]
    public void Constructor_ConNombreValido_ApuntaLaConexionALaBaseDePruebas()
    {
        var opciones = new TestDatabaseOptions(Servidor, "life_test");

        Assert.Equal("life_test", new SqlConnectionStringBuilder(opciones.ConnectionString).InitialCatalog);
    }

    [Fact]
    public void Constructor_ConNombreValido_UsaMasterParaLaConexionAdministrativa()
    {
        var opciones = new TestDatabaseOptions(Servidor, "life_test");

        Assert.Equal("master", new SqlConnectionStringBuilder(opciones.AdminConnectionString).InitialCatalog);
    }

    [Theory]
    [InlineData("life_dev")]
    [InlineData("life_prod")]
    [InlineData("master")]
    [InlineData("life_test; DROP DATABASE x")]
    public void Constructor_ConNombreQueNoEsDePruebas_LanzaExcepcion(string nombre)
    {
        var crear = () => new TestDatabaseOptions(Servidor, nombre);

        Assert.Throws<InvalidOperationException>(crear);
    }

    [Fact]
    public void Constructor_SinCadenaDeConexion_LanzaExcepcion()
    {
        var crear = () => new TestDatabaseOptions(null);

        Assert.Throws<InvalidOperationException>(crear);
    }

    [Fact]
    public void Constructor_ConCadenaQueApuntaAOtraBase_IgualUsaLaBaseDePruebas()
    {
        var conexionADesarrollo = Servidor + ";Database=life_dev";

        var opciones = new TestDatabaseOptions(conexionADesarrollo);

        Assert.Equal("life_test", new SqlConnectionStringBuilder(opciones.ConnectionString).InitialCatalog);
    }
}
