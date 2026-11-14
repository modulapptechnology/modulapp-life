using Life.Domain.Users;
using Life.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Life.API.Tests.Database;

public class IntegrityConventionsTests
{
    private static IForeignKey GetForeignKey<TEntity>(SampleDbContext context)
    {
        return context.Model.FindEntityType(typeof(TEntity))!.GetForeignKeys().Single();
    }

    [Fact]
    public void Model_WhenARelationDoesNotDeclareItsDeleteBehavior_RestrictsTheDeletion()
    {
        using var context = SampleDbContext.Create();

        var foreignKey = GetForeignKey<SampleChild>(context);

        Assert.Equal(DeleteBehavior.Restrict, foreignKey.DeleteBehavior);
    }

    [Fact]
    public void Model_WhenARelationDeclaresCascade_KeepsTheCascade()
    {
        using var context = SampleDbContext.Create();

        var foreignKey = GetForeignKey<SampleCascadeChild>(context);

        Assert.Equal(DeleteBehavior.Cascade, foreignKey.DeleteBehavior);
    }

    [Fact]
    public void Model_WhenARelationIsMapped_IndexesItsForeignKeyColumns()
    {
        using var context = SampleDbContext.Create();

        var unindexed = context.Model.GetEntityTypes()
            .SelectMany(entityType => entityType.GetForeignKeys())
            .Where(foreignKey => !foreignKey.DeclaringEntityType.GetIndexes().Any(index =>
                index.Properties.Take(foreignKey.Properties.Count).SequenceEqual(foreignKey.Properties)))
            .Select(foreignKey => $"{foreignKey.DeclaringEntityType.ClrType.Name}.{foreignKey.Properties[0].Name}")
            .ToList();

        Assert.Empty(unindexed);
    }

    [Fact]
    public void Model_WhenTheUsersAreMapped_RestrictsTheDeletionOfEveryRelationWithThem()
    {
        using var context = SampleDbContext.Create();

        var relations = context.Model.FindEntityType(typeof(User))!.GetReferencingForeignKeys().ToList();

        Assert.All(relations, relation => Assert.Equal(DeleteBehavior.Restrict, relation.DeleteBehavior));
    }

    [Fact]
    public void Model_WhenTheUsersAreMapped_LimitsTheStatusToTheKnownValues()
    {
        using var context = SampleDbContext.Create();

        var constraint = context.GetService<IDesignTimeModel>().Model
            .FindEntityType(typeof(User))!.GetCheckConstraints().Single();

        Assert.Equal(
            (UserConfiguration.StatusCheckConstraintName, "[Status] IN (0, 1, 2)"),
            (constraint.Name, constraint.Sql));
    }
}
