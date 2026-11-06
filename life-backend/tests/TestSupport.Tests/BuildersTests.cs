using TestSupport.Builders;
using TestSupport.Seed;

namespace TestSupport.Tests;

public class BuildersTests
{
    [Fact]
    public void Build_WithoutFixedValues_GeneratesUsersWithUniqueEmail()
    {
        var builder = new TestUserBuilder();

        var emails = new[] { builder.Build().Email, builder.Build().Email };

        Assert.Equal(2, emails.Distinct().Count());
    }

    [Fact]
    public void Build_WithFixedValues_KeepsTheValues()
    {
        var builder = new TestUserBuilder().WithEmail("a@life.test").WithName("Ana");

        var user = builder.Build();

        Assert.Equal(("a@life.test", "Ana"), (user.Email, user.Name));
    }

    [Fact]
    public void Build_WithOwner_AssignsTheOwnerToTheWorkspace()
    {
        var owner = new TestUserBuilder().Build();
        var builder = new TestWorkspaceBuilder(owner.Id);

        var workspace = builder.Build();

        Assert.Equal(owner.Id, workspace.OwnerId);
    }

    [Fact]
    public void SeedData_Workspaces_HaveASeedUserAsOwner()
    {
        var userIds = SeedData.Users.Select(user => user.Id);

        var ownerIds = SeedData.Workspaces.Select(workspace => workspace.OwnerId);

        Assert.All(ownerIds, ownerId => Assert.Contains(ownerId, userIds));
    }

    [Fact]
    public void SeedData_Users_HaveDistinctEmails()
    {
        var users = SeedData.Users;

        var emails = users.Select(user => user.Email).Distinct();

        Assert.Equal(users.Count, emails.Count());
    }
}
