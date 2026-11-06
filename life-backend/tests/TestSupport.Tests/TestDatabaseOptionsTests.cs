using Microsoft.Data.SqlClient;

namespace TestSupport.Tests;

public class TestDatabaseOptionsTests
{
    private const string Server = "Server=localhost,1433;User Id=sa;Password=x;TrustServerCertificate=True";

    [Fact]
    public void Constructor_WithValidName_PointsTheConnectionToTheTestDatabase()
    {
        var options = new TestDatabaseOptions(Server, "life_test");

        Assert.Equal("life_test", new SqlConnectionStringBuilder(options.ConnectionString).InitialCatalog);
    }

    [Fact]
    public void Constructor_WithValidName_UsesMasterForTheAdminConnection()
    {
        var options = new TestDatabaseOptions(Server, "life_test");

        Assert.Equal("master", new SqlConnectionStringBuilder(options.AdminConnectionString).InitialCatalog);
    }

    [Theory]
    [InlineData("life_dev")]
    [InlineData("life_prod")]
    [InlineData("master")]
    [InlineData("life_test; DROP DATABASE x")]
    public void Constructor_WithNonTestName_ThrowsException(string name)
    {
        var create = () => new TestDatabaseOptions(Server, name);

        Assert.Throws<InvalidOperationException>(create);
    }

    [Fact]
    public void Constructor_WithoutConnectionString_ThrowsException()
    {
        var create = () => new TestDatabaseOptions(null);

        Assert.Throws<InvalidOperationException>(create);
    }

    [Fact]
    public void Constructor_WithConnectionStringPointingToAnotherDatabase_StillUsesTheTestDatabase()
    {
        var developmentConnection = Server + ";Database=life_dev";

        var options = new TestDatabaseOptions(developmentConnection);

        Assert.Equal("life_test", new SqlConnectionStringBuilder(options.ConnectionString).InitialCatalog);
    }
}
