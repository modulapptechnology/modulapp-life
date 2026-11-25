using Life.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Life.API.Tests.Database;

public class MigrationsTests
{
    private const string _connectionString = "Host=localhost;Port=5432;Database=life_unit;Username=none;Password=none";

    private static LifeDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<LifeDbContext>().UseNpgsql(_connectionString).Options;

        return new LifeDbContext(options);
    }

    [Fact]
    public void GetMigrations_WhenTheProjectHasMigrations_IncludesTheInitialMigration()
    {
        using var context = CreateContext();

        var migrations = context.Database.GetMigrations();

        Assert.Contains(migrations, migration => migration.EndsWith("_InitialCreate", StringComparison.Ordinal));
    }

    [Fact]
    public void HasPendingModelChanges_WhenTheModelMatchesTheSnapshot_ReturnsFalse()
    {
        using var context = CreateContext();

        var hasPendingChanges = context.Database.HasPendingModelChanges();

        Assert.False(hasPendingChanges);
    }

    [Theory]
    [InlineData("nvarchar")]
    [InlineData("rowversion")]
    [InlineData("uniqueidentifier")]
    [InlineData("datetime2")]
    public void GenerateScript_WhenAllMigrationsAreApplied_ContainsNoSqlServerTypes(string sqlServerType)
    {
        using var context = CreateContext();
        var migrator = context.GetService<IMigrator>();

        var script = migrator.GenerateScript();

        Assert.DoesNotContain(sqlServerType, script, StringComparison.Ordinal);
    }
}
