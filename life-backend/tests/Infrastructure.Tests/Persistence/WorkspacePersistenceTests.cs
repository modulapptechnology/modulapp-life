using Domain.Users;
using Domain.Workspaces;
using Infrastructure.Tests.Support;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Support;

namespace Infrastructure.Tests.Persistence;

[Collection(MigratedDatabaseCollection.Name)]
public class WorkspacePersistenceTests(MigratedDatabaseFixture database)
{
    private static User NewUser()
    {
        return new User { Email = $"{Guid.NewGuid():N}@life.test", Name = "Test user" };
    }

    private static Workspace NewWorkspace(Guid ownerId)
    {
        return new Workspace { Name = "Home", Type = WorkspaceType.Household, OwnerId = ownerId };
    }

    private static WorkspaceMember NewMember(Guid workspaceId, Guid userId, WorkspaceRole role = WorkspaceRole.Member)
    {
        return new WorkspaceMember { WorkspaceId = workspaceId, UserId = userId, Role = role };
    }

    private static async Task<(User Owner, Workspace Workspace)> SaveWorkspaceAsync(LifeDbContext context)
    {
        var owner = NewUser();
        context.Users.Add(owner);
        await context.SaveChangesAsync();
        var workspace = NewWorkspace(owner.Id);
        context.Workspaces.Add(workspace);
        await context.SaveChangesAsync();

        return (owner, workspace);
    }

    [RequiresTestDatabaseFact]
    public async Task SaveChangesAsync_WhenAWorkspaceHasMembersAndInvitations_LoadsThemWithTheWorkspace()
    {
        await using var writer = database.CreateContext();
        var (owner, workspace) = await SaveWorkspaceAsync(writer);
        writer.WorkspaceMembers.Add(NewMember(workspace.Id, owner.Id, WorkspaceRole.Owner));
        writer.WorkspaceInvitations.Add(new WorkspaceInvitation
        {
            WorkspaceId = workspace.Id,
            Email = "guest@life.test",
            TokenHash = Guid.NewGuid().ToString("N"),
            ExpiresAt = DateTime.UtcNow.AddDays(7),
        });
        await writer.SaveChangesAsync();

        await using var reader = database.CreateContext();
        var stored = await reader.Workspaces
            .Include(candidate => candidate.Members)
            .Include(candidate => candidate.Invitations)
            .AsNoTracking()
            .SingleAsync(candidate => candidate.Id == workspace.Id);

        Assert.Equal(
            (owner.Id, 1, 1),
            (stored.OwnerId, stored.Members.Count, stored.Invitations.Count));
    }

    [RequiresTestDatabaseFact]
    public async Task SaveChangesAsync_WhenTheWorkspaceHasNoOwner_RejectsTheWorkspace()
    {
        await using var context = database.CreateContext();
        context.Workspaces.Add(NewWorkspace(Guid.Empty));

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [RequiresTestDatabaseFact]
    public async Task SaveChangesAsync_WhenTheOwnerDoesNotExist_RejectsTheWorkspace()
    {
        await using var context = database.CreateContext();
        context.Workspaces.Add(NewWorkspace(Guid.NewGuid()));

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [RequiresTestDatabaseFact]
    public async Task SaveChangesAsync_WhenTheUserIsAlreadyAMemberOfTheWorkspace_RejectsTheDuplicate()
    {
        await using var context = database.CreateContext();
        var (owner, workspace) = await SaveWorkspaceAsync(context);
        context.WorkspaceMembers.Add(NewMember(workspace.Id, owner.Id));
        await context.SaveChangesAsync();
        context.WorkspaceMembers.Add(NewMember(workspace.Id, owner.Id, WorkspaceRole.Admin));

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [RequiresTestDatabaseFact]
    public async Task SaveChangesAsync_WhenTheMembershipWasRemoved_AllowsJoiningAgain()
    {
        await using var context = database.CreateContext();
        var (owner, workspace) = await SaveWorkspaceAsync(context);
        var removed = NewMember(workspace.Id, owner.Id);
        context.WorkspaceMembers.Add(removed);
        await context.SaveChangesAsync();
        context.WorkspaceMembers.Remove(removed);
        await context.SaveChangesAsync();

        context.WorkspaceMembers.Add(NewMember(workspace.Id, owner.Id));
        var written = await context.SaveChangesAsync();

        Assert.Equal(1, written);
    }

    [RequiresTestDatabaseFact]
    public async Task SaveChangesAsync_WhenTheUserIsMemberOfDifferentWorkspaces_AcceptsBothMemberships()
    {
        await using var context = database.CreateContext();
        var (owner, first) = await SaveWorkspaceAsync(context);
        var second = NewWorkspace(owner.Id);
        context.Workspaces.Add(second);
        await context.SaveChangesAsync();
        context.WorkspaceMembers.Add(NewMember(first.Id, owner.Id));
        context.WorkspaceMembers.Add(NewMember(second.Id, owner.Id));

        var written = await context.SaveChangesAsync();

        Assert.Equal(2, written);
    }

    [RequiresTestDatabaseFact]
    public async Task SaveChangesAsync_WhenTheWorkspaceAlreadyHasAnOwnerMember_RejectsASecondOne()
    {
        await using var context = database.CreateContext();
        var (owner, workspace) = await SaveWorkspaceAsync(context);
        var other = NewUser();
        context.Users.Add(other);
        await context.SaveChangesAsync();
        context.WorkspaceMembers.Add(NewMember(workspace.Id, owner.Id, WorkspaceRole.Owner));
        await context.SaveChangesAsync();
        context.WorkspaceMembers.Add(NewMember(workspace.Id, other.Id, WorkspaceRole.Owner));

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [RequiresTestDatabaseFact]
    public async Task SaveChangesAsync_WhenTheMemberReferencesAMissingWorkspace_RejectsTheMember()
    {
        await using var context = database.CreateContext();
        var user = NewUser();
        context.Users.Add(user);
        await context.SaveChangesAsync();
        context.WorkspaceMembers.Add(NewMember(Guid.NewGuid(), user.Id));

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [RequiresTestDatabaseFact]
    public async Task SaveChangesAsync_WhenTheRoleIsNotAKnownValue_RejectsTheMember()
    {
        await using var context = database.CreateContext();
        var (owner, workspace) = await SaveWorkspaceAsync(context);
        context.WorkspaceMembers.Add(NewMember(workspace.Id, owner.Id, (WorkspaceRole)99));

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [RequiresTestDatabaseFact]
    public async Task SaveChangesAsync_WhenTheWorkspaceTypeIsNotAKnownValue_RejectsTheWorkspace()
    {
        await using var context = database.CreateContext();
        var owner = NewUser();
        context.Users.Add(owner);
        await context.SaveChangesAsync();
        var workspace = NewWorkspace(owner.Id);
        workspace.Type = (WorkspaceType)99;
        context.Workspaces.Add(workspace);

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [RequiresTestDatabaseFact]
    public async Task SaveChangesAsync_WhenTheInvitationStatusIsNotAKnownValue_RejectsTheInvitation()
    {
        await using var context = database.CreateContext();
        var (_, workspace) = await SaveWorkspaceAsync(context);
        context.WorkspaceInvitations.Add(new WorkspaceInvitation
        {
            WorkspaceId = workspace.Id,
            Email = "guest@life.test",
            TokenHash = Guid.NewGuid().ToString("N"),
            Status = (InvitationStatus)99,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
        });

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [RequiresTestDatabaseFact]
    public async Task ExecuteSqlAsync_WhenTheWorkspaceHasMembersAndItsRowIsDeleted_RejectsTheDeletion()
    {
        await using var context = database.CreateContext();
        var (owner, workspace) = await SaveWorkspaceAsync(context);
        context.WorkspaceMembers.Add(NewMember(workspace.Id, owner.Id, WorkspaceRole.Owner));
        await context.SaveChangesAsync();

        var delete = () => context.Database.ExecuteSqlAsync($"DELETE FROM Workspaces WHERE Id = {workspace.Id}");

        await Assert.ThrowsAsync<SqlException>(delete);
    }
}
