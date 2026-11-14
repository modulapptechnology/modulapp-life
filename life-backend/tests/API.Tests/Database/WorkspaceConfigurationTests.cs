using Life.Domain.Users;
using Life.Domain.Workspaces;
using Life.Persistence;
using Life.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Life.API.Tests.Database;

public class WorkspaceConfigurationTests
{
    private static IEntityType GetEntityType<TEntity>(LifeDbContext context)
    {
        return context.GetService<IDesignTimeModel>().Model.FindEntityType(typeof(TEntity))!;
    }

    [Fact]
    public void Model_WhenWorkspacesAreMapped_RequiresAnOwnerAmongTheUsersAndRestrictsItsDeletion()
    {
        using var context = SampleDbContext.Create();

        var entityType = GetEntityType<Workspace>(context);
        var foreignKey = entityType.GetForeignKeys().Single();

        Assert.Equal(
            (typeof(User), true, DeleteBehavior.Restrict, false),
            (foreignKey.PrincipalEntityType.ClrType,
                foreignKey.IsRequired,
                foreignKey.DeleteBehavior,
                entityType.FindProperty(nameof(Workspace.OwnerId))!.IsNullable));
    }

    [Fact]
    public void Model_WhenMembersAreMapped_KeepsTheMembershipUniqueAmongActiveMembers()
    {
        using var context = SampleDbContext.Create();

        var index = GetEntityType<WorkspaceMember>(context).GetIndexes()
            .Single(candidate => candidate.IsUnique && candidate.Properties.Count == 2);

        Assert.Equal(
            ("WorkspaceId,UserId", "[IsDeleted] = 0"),
            (string.Join(",", index.Properties.Select(property => property.Name)), index.GetFilter()));
    }

    [Fact]
    public void Model_WhenMembersAreMapped_AllowsOnlyOneOwnerPerWorkspace()
    {
        using var context = SampleDbContext.Create();

        var index = GetEntityType<WorkspaceMember>(context).GetIndexes()
            .Single(candidate => candidate.GetDatabaseName() == WorkspaceMemberConfiguration.SingleOwnerIndexName);

        Assert.Equal(
            (true, "WorkspaceId", "[IsDeleted] = 0 AND [Role] = 2"),
            (index.IsUnique, index.Properties.Single().Name, index.GetFilter()));
    }

    [Fact]
    public void Model_WhenWorkspaceEntitiesAreMapped_RestrictsTheDeletionOfEveryRelation()
    {
        using var context = SampleDbContext.Create();

        var relations = new[] { typeof(Workspace), typeof(WorkspaceMember), typeof(WorkspaceInvitation) }
            .SelectMany(entityType => context.Model.FindEntityType(entityType)!.GetForeignKeys())
            .ToList();

        Assert.All(relations, relation => Assert.Equal(DeleteBehavior.Restrict, relation.DeleteBehavior));
    }

    [Fact]
    public void Model_WhenWorkspaceEntitiesAreMapped_LimitsEveryEnumToItsKnownValues()
    {
        using var context = SampleDbContext.Create();

        var constraints = new[]
            {
                (Entity: typeof(Workspace), Entry: GetEntityType<Workspace>(context)),
                (Entity: typeof(WorkspaceMember), Entry: GetEntityType<WorkspaceMember>(context)),
                (Entity: typeof(WorkspaceInvitation), Entry: GetEntityType<WorkspaceInvitation>(context)),
            }
            .SelectMany(item => item.Entry.GetCheckConstraints())
            .ToDictionary(constraint => constraint.Name!, constraint => constraint.Sql);

        Assert.Equal(
            new Dictionary<string, string>
            {
                [WorkspaceConfiguration.TypeCheckConstraintName] = "[Type] IN (0, 1, 2)",
                [WorkspaceConfiguration.StatusCheckConstraintName] = "[Status] IN (0, 1)",
                [WorkspaceMemberConfiguration.RoleCheckConstraintName] = "[Role] IN (0, 1, 2)",
                [WorkspaceInvitationConfiguration.RoleCheckConstraintName] = "[Role] IN (0, 1, 2)",
                [WorkspaceInvitationConfiguration.StatusCheckConstraintName] = "[Status] IN (0, 1, 2, 3)",
            },
            constraints);
    }
}
