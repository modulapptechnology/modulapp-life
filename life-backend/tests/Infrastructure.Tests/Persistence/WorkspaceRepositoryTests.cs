using Domain.Users;
using Domain.Workspaces;
using Infrastructure.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Support;

namespace Infrastructure.Tests.Persistence;

[Collection(MigratedDatabaseCollection.Name)]
public class WorkspaceRepositoryTests(MigratedDatabaseFixture database)
{
    private static async Task<User> SaveUserAsync(LifeDbContext context)
    {
        var user = new User { Email = $"{Guid.NewGuid():N}@life.test", Name = "Test user" };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        return user;
    }

    private static Workspace NewWorkspace(Guid ownerId)
    {
        return new Workspace
        {
            Name = "Home",
            Description = "Shared tasks",
            OwnerId = ownerId,
            Members = [new WorkspaceMember { UserId = ownerId, Role = WorkspaceRole.Owner }],
        };
    }

    [RequiresTestDatabaseFact]
    public async Task Add_WithAWorkspaceAndItsOwnerMember_StoresBothAndTheDescription()
    {
        await using var context = database.CreateContext();
        var owner = await SaveUserAsync(context);
        var workspace = NewWorkspace(owner.Id);
        var repository = new WorkspaceRepository(context);

        repository.Add(workspace);
        await context.SaveChangesAsync();

        await using var reader = database.CreateContext();
        var stored = await reader.Workspaces.AsNoTracking()
            .Include(candidate => candidate.Members)
            .SingleAsync(candidate => candidate.Id == workspace.Id);
        var member = Assert.Single(stored.Members);
        Assert.Equal(
            ("Shared tasks", owner.Id, WorkspaceRole.Owner),
            (stored.Description, member.UserId, member.Role));
    }

    [RequiresTestDatabaseFact]
    public async Task CountOwnedAsync_WithWorkspacesOfSeveralOwners_CountsOnlyThoseOfTheOwner()
    {
        await using var context = database.CreateContext();
        var owner = await SaveUserAsync(context);
        var other = await SaveUserAsync(context);
        context.Workspaces.AddRange(NewWorkspace(owner.Id), NewWorkspace(owner.Id), NewWorkspace(other.Id));
        await context.SaveChangesAsync();
        var repository = new WorkspaceRepository(context);

        var count = await repository.CountOwnedAsync(owner.Id);

        Assert.Equal(2, count);
    }

    [RequiresTestDatabaseFact]
    public async Task CountOwnedAsync_WithADeletedWorkspace_DoesNotCountIt()
    {
        await using var context = database.CreateContext();
        var owner = await SaveUserAsync(context);
        var deleted = NewWorkspace(owner.Id);
        deleted.IsDeleted = true;
        context.Workspaces.AddRange(NewWorkspace(owner.Id), deleted);
        await context.SaveChangesAsync();
        var repository = new WorkspaceRepository(context);

        var count = await repository.CountOwnedAsync(owner.Id);

        Assert.Equal(1, count);
    }

    [RequiresTestDatabaseFact]
    public async Task FindByMemberAsync_WithAMemberOfTheWorkspace_ReturnsItsSummaryWithTheRoleOfTheMember()
    {
        await using var context = database.CreateContext();
        var owner = await SaveUserAsync(context);
        var workspace = NewWorkspace(owner.Id);
        context.Workspaces.Add(workspace);
        await context.SaveChangesAsync();
        var repository = new WorkspaceRepository(context);

        var summary = await repository.FindByMemberAsync(workspace.Id, owner.Id);

        Assert.Equal((workspace.Id, WorkspaceRole.Owner), (summary?.Id, summary?.Role));
    }

    [RequiresTestDatabaseFact]
    public async Task FindByMemberAsync_WithAUserWhoIsNotAMember_ReturnsNull()
    {
        await using var context = database.CreateContext();
        var owner = await SaveUserAsync(context);
        var outsider = await SaveUserAsync(context);
        var workspace = NewWorkspace(owner.Id);
        context.Workspaces.Add(workspace);
        await context.SaveChangesAsync();
        var repository = new WorkspaceRepository(context);

        var summary = await repository.FindByMemberAsync(workspace.Id, outsider.Id);

        Assert.Null(summary);
    }

    [RequiresTestDatabaseFact]
    public async Task ListByMemberAsync_WithWorkspacesOfSeveralUsers_ReturnsOnlyTheOnesOfTheMemberOrderedByName()
    {
        await using var context = database.CreateContext();
        var owner = await SaveUserAsync(context);
        var other = await SaveUserAsync(context);
        var work = NewWorkspace(owner.Id);
        work.Name = "Work";
        var home = NewWorkspace(owner.Id);
        home.Name = "Home";
        context.Workspaces.AddRange(work, home, NewWorkspace(other.Id));
        await context.SaveChangesAsync();
        var repository = new WorkspaceRepository(context);

        var summaries = await repository.ListByMemberAsync(owner.Id);

        Assert.Equal(["Home", "Work"], summaries.Select(summary => summary.Name));
    }
}
