using Microsoft.Data.SqlClient;

namespace Support.Tests;

public class TestDatabaseOptionsTests
{
    private const string _server = "Server=localhost,1433;User Id=sa;Password=x;TrustServerCertificate=True";

    [Theory]
    [InlineData("ModulLifeApiTest")]
    [InlineData("ModulLifeSupportTest")]
    public void Constructor_WithValidName_PointsTheConnectionToTheTestDatabase(string name)
    {
        var options = new TestDatabaseOptions(_server, name);

        Assert.Equal(name, new SqlConnectionStringBuilder(options.ConnectionString).InitialCatalog);
    }

    [Fact]
    public void Constructor_WithValidName_UsesMasterForTheAdminConnection()
    {
        var options = new TestDatabaseOptions(_server, "ModulLifeApiTest");

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
        var create = () => new TestDatabaseOptions(_server, name);

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
        var developmentConnection = _server + ";Database=ModulLifeDev";

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
        var api = TestDatabaseOptions.ForProject(_server, "Api");
        var support = TestDatabaseOptions.ForProject(_server, "Support");

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
        var create = () => TestDatabaseOptions.ForProject(_server, "Api; DROP DATABASE x");

        Assert.Throws<InvalidOperationException>(create);
    }
}
