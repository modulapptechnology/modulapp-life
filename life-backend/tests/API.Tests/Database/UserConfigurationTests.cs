using Life.Domain.Common;
using Life.Domain.Users;
using Life.Persistence;
using Life.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Life.API.Tests.Database;

public class UserConfigurationTests
{
    private static IEntityType GetEntityType<TEntity>(LifeDbContext context)
    {
        return context.Model.FindEntityType(typeof(TEntity))!;
    }

    [Fact]
    public void Model_WhenUsersAreMapped_UsesTheBaseConventions()
    {
        using var context = SampleDbContext.Create();

        var entityType = GetEntityType<User>(context);

        Assert.Equal(
            (true, typeof(Guid), true, "rowversion"),
            (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType),
                entityType.FindPrimaryKey()!.Properties.Single().ClrType,
                entityType.GetDeclaredQueryFilters().Count > 0,
                entityType.FindProperty(nameof(User.RowVersion))!.GetColumnType()));
    }

    [Fact]
    public void Model_WhenUsersAreMapped_HasAUniqueEmailIndexAmongActiveUsers()
    {
        using var context = SampleDbContext.Create();

        var index = GetEntityType<User>(context).GetIndexes()
            .Single(candidate => candidate.Properties.Single().Name == nameof(User.Email));

        Assert.Equal((true, "[IsDeleted] = 0"), (index.IsUnique, index.GetFilter()));
    }

    [Fact]
    public void Model_WhenUsersAreMapped_ComparesTheEmailIgnoringCase()
    {
        using var context = SampleDbContext.Create();

        var email = context.GetService<IDesignTimeModel>().Model
            .FindEntityType(typeof(User))!.FindProperty(nameof(User.Email))!;

        Assert.Equal(
            (UserConfiguration.CaseInsensitiveCollation, false),
            (email.GetCollation(), email.IsNullable));
    }

    [Fact]
    public void Model_WhenUsersAreMapped_StoresOnlyThePasswordHash()
    {
        using var context = SampleDbContext.Create();

        var passwordColumns = GetEntityType<User>(context).GetProperties()
            .Select(property => property.Name)
            .Where(name => name.Contains("Password", StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.Equal(nameof(User.PasswordHash), string.Join(",", passwordColumns));
    }

    [Fact]
    public void Model_WhenExternalAccountsAreMapped_LinksEachOneToItsUserAndKeepsTheProviderAccountUnique()
    {
        using var context = SampleDbContext.Create();

        var entityType = GetEntityType<ExternalAccount>(context);
        var foreignKey = entityType.GetForeignKeys().Single();
        var uniqueIndex = entityType.GetIndexes().Single(candidate => candidate.IsUnique);

        Assert.Equal(
            (typeof(User), true, "Provider,ProviderUserId"),
            (foreignKey.PrincipalEntityType.ClrType,
                foreignKey.IsRequired,
                string.Join(",", uniqueIndex.Properties.Select(property => property.Name))));
    }
}
