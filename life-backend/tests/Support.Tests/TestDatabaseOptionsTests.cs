using Microsoft.Data.SqlClient;

namespace Support.Tests;

public class TestDatabaseOptionsTests
{
    private const string Server = "Server=localhost,1433;User Id=sa;Password=x;TrustServerCertificate=True";

    [Theory]
    [InlineData("ModulLifeApiTest")]
    [InlineData("ModulLifeSupportTest")]
    public void Constructor_WithValidName_PointsTheConnectionToTheTestDatabase(string name)
    {
        var options = new TestDatabaseOptions(Server, name);

        Assert.Equal(name, new SqlConnectionStringBuilder(options.ConnectionString).InitialCatalog);
    }

    [Fact]
    public void Constructor_WithValidName_UsesMasterForTheAdminConnection()
    {
        var options = new TestDatabaseOptions(Server, "ModulLifeApiTest");

        Assert.Equal("master", new SqlConnectionStringBuilder(options.AdminConnectionString).InitialCatalog);
    }

    [Theory]
    [InlineData("ModulLifeDev")]
    [InlineData("ModulLifeProd")]
    [InlineData("master")]
    [InlineData("ModulLifeApi")]
    [InlineData("life_test")]
    [InlineData("ModulLifeApiTest; DROP DATABASE x")]
    public void Constructor_WithNameNotEndingInTest_ThrowsException(string name)
    {
        var create = () => new TestDatabaseOptions(Server, name);

        Assert.Throws<InvalidOperationException>(create);
    }

    [Fact]
    public void Constructor_WithoutConnectionString_ThrowsException()
    {
        var create = () => new TestDatabaseOptions(null, "ModulLifeApiTest");

        Assert.Throws<InvalidOperationException>(create);
    }

    [Fact]
    public void Constructor_WithConnectionStringPointingToAnotherDatabase_StillUsesTheTestDatabase()
    {
        var developmentConnection = Server + ";Database=ModulLifeDev";

        var options = new TestDatabaseOptions(developmentConnection, "ModulLifeApiTest");

        Assert.Equal(
            "ModulLifeApiTest",
            new SqlConnectionStringBuilder(options.ConnectionString).InitialCatalog);
    }

    [Theory]
    [InlineData("Api", "ModulLifeApiTest")]
    [InlineData("Support", "ModulLifeSupportTest")]
    public void NameForProject_WithProjectName_BuildsTheConventionalName(string project, string expected)
    {
        var name = TestDatabaseOptions.NameForProject(project);

        Assert.Equal(expected, name);
    }

    [Fact]
    public void ForProject_WithDifferentProjects_UsesDifferentDatabases()
    {
        var api = TestDatabaseOptions.ForProject(Server, "Api");
        var support = TestDatabaseOptions.ForProject(Server, "Support");

        Assert.NotEqual(api.DatabaseName, support.DatabaseName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void NameForProject_WithEmptyProject_ThrowsException(string project)
    {
        var build = () => TestDatabaseOptions.NameForProject(project);

        Assert.Throws<ArgumentException>(build);
    }

    [Fact]
    public void ForProject_WithInvalidProjectName_ThrowsException()
    {
        var create = () => TestDatabaseOptions.ForProject(Server, "Api; DROP DATABASE x");

        Assert.Throws<InvalidOperationException>(create);
    }
}
