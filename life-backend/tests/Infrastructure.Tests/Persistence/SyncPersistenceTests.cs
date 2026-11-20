using Life.Application.Sync;
using Life.Domain.Sync;
using Life.Domain.Users;
using Life.Domain.Workspaces;
using Life.Infrastructure.Tests.Support;
using Life.Persistence;
using Life.Persistence.Sync;
using Life.Support;
using Microsoft.EntityFrameworkCore;

namespace Life.Infrastructure.Tests.Persistence;

[Collection(MigratedDatabaseCollection.Name)]
public class SyncPersistenceTests(MigratedDatabaseFixture database)
{
    private static async Task<(User User, Workspace Workspace)> SaveWorkspaceAsync(LifeDbContext context)
    {
        var user = new User { Email = $"{Guid.NewGuid():N}@life.test", Name = "Test user" };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        var workspace = new Workspace
        {
            Name = "Home",
            OwnerId = user.Id,
            Members = [new WorkspaceMember { UserId = user.Id, Role = WorkspaceRole.Owner }],
        };
        context.Workspaces.Add(workspace);
        await context.SaveChangesAsync();

        return (user, workspace);
    }

    private static SyncOperationLog NewEntry(Guid workspaceId, Guid userId, Guid operationId)
    {
        return new SyncOperationLog
        {
            OperationId = operationId,
            WorkspaceId = workspaceId,
            UserId = userId,
            EntityType = "task",
            EntityId = Guid.NewGuid(),
            Kind = SyncOperationKind.Create,
            Status = SyncOperationStatus.Applied,
        };
    }

    [RequiresTestDatabaseFact]
    public async Task AddAsync_WithAnOperationAlreadyStored_ThrowsADuplicateOperationException()
    {
        await using var context = database.CreateContext();
        var (user, workspace) = await SaveWorkspaceAsync(context);
        var operationId = Guid.NewGuid();
        var repository = new SyncOperationLogRepository(context);
        await repository.AddAsync(NewEntry(workspace.Id, user.Id, operationId));

        var repeated = repository.AddAsync(NewEntry(workspace.Id, user.Id, operationId));

        await Assert.ThrowsAsync<DuplicateSyncOperationException>(() => repeated);
    }

    [RequiresTestDatabaseFact]
    public async Task FindAsync_WithAStoredOperation_ReturnsItsOriginalResult()
    {
        await using var context = database.CreateContext();
        var (user, workspace) = await SaveWorkspaceAsync(context);
        var entry = NewEntry(workspace.Id, user.Id, Guid.NewGuid());
        entry.Status = SyncOperationStatus.Rejected;
        entry.ErrorCode = "read_only";
        var repository = new SyncOperationLogRepository(context);
        await repository.AddAsync(entry);

        var found = await repository.FindAsync(workspace.Id, user.Id, entry.OperationId);

        Assert.Equal((SyncOperationStatus.Rejected, "read_only"), (found?.Status, found?.ErrorCode));
    }

    [RequiresTestDatabaseFact]
    public async Task SaveChangesAsync_WithARepeatedRecordId_IsRejectedByTheDatabase()
    {
        await using var context = database.CreateContext();
        var (user, workspace) = await SaveWorkspaceAsync(context);
        await using var other = database.CreateContext();
        other.Workspaces.Add(new Workspace { Id = workspace.Id, Name = "Copy", OwnerId = user.Id });

        var repeated = other.SaveChangesAsync();

        await Assert.ThrowsAsync<DbUpdateException>(() => repeated);
    }

    [RequiresTestDatabaseFact]
    public async Task ListChangesAsync_AfterTheLastVersion_ReturnsNothingUntilTheWorkspaceChanges()
    {
        await using var context = database.CreateContext();
        var (user, workspace) = await SaveWorkspaceAsync(context);
        var handler = new WorkspaceSyncHandler(context);
        var syncContext = new SyncContext(workspace.Id, user.Id);
        var first = await handler.ListChangesAsync(syncContext, new byte[SyncCursor.Length], 10);
        var tracked = await context.Workspaces.SingleAsync(candidate => candidate.Id == workspace.Id);
        var unchanged = await handler.ListChangesAsync(syncContext, first[0].RowVersion, 10);
        tracked.Name = "Renamed";
        await context.SaveChangesAsync();

        var changed = await handler.ListChangesAsync(syncContext, first[0].RowVersion, 10);

        Assert.Equal((1, 0, 1), (first.Count, unchanged.Count, changed.Count));
    }

    [RequiresTestDatabaseFact]
    public async Task ListChangesAsync_WithADeletedWorkspace_ReturnsTheDeletionWithoutData()
    {
        await using var context = database.CreateContext();
        var (user, workspace) = await SaveWorkspaceAsync(context);
        await using var deleter = database.CreateContext();
        var tracked = await deleter.Workspaces.SingleAsync(candidate => candidate.Id == workspace.Id);
        deleter.Workspaces.Remove(tracked);
        await deleter.SaveChangesAsync();
        var handler = new WorkspaceSyncHandler(context);

        var changes = await handler.ListChangesAsync(
            new SyncContext(workspace.Id, user.Id),
            new byte[SyncCursor.Length],
            10);

        var change = Assert.Single(changes);
        Assert.Equal((true, null), (change.IsDeleted, change.Data));
    }

    [RequiresTestDatabaseFact]
    public async Task ListChangesAsync_ForAnotherWorkspace_ReturnsNoChanges()
    {
        await using var context = database.CreateContext();
        var (user, _) = await SaveWorkspaceAsync(context);
        await SaveWorkspaceAsync(context);
        var handler = new WorkspaceSyncHandler(context);

        var changes = await handler.ListChangesAsync(
            new SyncContext(Guid.NewGuid(), user.Id),
            new byte[SyncCursor.Length],
            10);

        Assert.Empty(changes);
    }
}
