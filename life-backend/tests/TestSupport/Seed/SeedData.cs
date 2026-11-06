using TestSupport.Builders;

namespace TestSupport.Seed;

public static class SeedData
{
    public static readonly TestUser Owner = new TestUserBuilder()
        .WithId(new Guid("00000000-0000-0000-0000-000000000001"))
        .WithEmail("owner@life.test")
        .WithName("Test owner")
        .Build();

    public static readonly TestUser Member = new TestUserBuilder()
        .WithId(new Guid("00000000-0000-0000-0000-000000000002"))
        .WithEmail("member@life.test")
        .WithName("Test member")
        .Build();

    public static readonly TestWorkspace MainWorkspace = new TestWorkspaceBuilder(Owner.Id)
        .WithId(new Guid("00000000-0000-0000-0000-0000000000a1"))
        .WithName("Main test workspace")
        .Build();

    public static IReadOnlyList<TestUser> Users { get; } = [Owner, Member];

    public static IReadOnlyList<TestWorkspace> Workspaces { get; } = [MainWorkspace];
}
