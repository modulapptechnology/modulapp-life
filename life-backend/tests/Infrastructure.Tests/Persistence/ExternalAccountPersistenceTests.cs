using Life.Domain.Users;
using Life.Infrastructure.Tests.Support;
using Life.Support;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Life.Infrastructure.Tests.Persistence;

[Collection(MigratedDatabaseCollection.Name)]
public class ExternalAccountPersistenceTests(MigratedDatabaseFixture database)
{
    private static User NewUser()
    {
        return new User { Email = $"{Guid.NewGuid():N}@life.test", Name = "Test user" };
    }

    private static ExternalAccount NewAccount(Guid userId, string providerUserId)
    {
        return new ExternalAccount { UserId = userId, Provider = "google", ProviderUserId = providerUserId };
    }

    private static User NewUserWithAccount()
    {
        var user = NewUser();
        user.ExternalAccounts.Add(NewAccount(Guid.Empty, $"{Guid.NewGuid():N}"));

        return user;
    }

    [RequiresTestDatabaseFact]
    public async Task SaveChangesAsync_WhenAnAccountIsAddedToAUser_LoadsItWithTheUser()
    {
        var user = NewUserWithAccount();
        await using var writer = database.CreateContext();
        writer.Users.Add(user);
        await writer.SaveChangesAsync();

        await using var reader = database.CreateContext();
        var stored = await reader.Users.Include(candidate => candidate.ExternalAccounts)
            .AsNoTracking()
            .SingleAsync(candidate => candidate.Id == user.Id);

        Assert.Equal((1, user.Id), (stored.ExternalAccounts.Count, stored.ExternalAccounts.Single().UserId));
    }

    [RequiresTestDatabaseFact]
    public async Task SaveChangesAsync_WhenTheUserDoesNotExist_RejectsTheAccount()
    {
        await using var context = database.CreateContext();
        context.ExternalAccounts.Add(NewAccount(Guid.NewGuid(), $"{Guid.NewGuid():N}"));

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [RequiresTestDatabaseFact]
    public async Task SaveChangesAsync_WhenTheProviderAccountIsAlreadyLinked_RejectsTheDuplicate()
    {
        var providerUserId = $"{Guid.NewGuid():N}";
        var first = NewUser();
        var second = NewUser();
        await using var context = database.CreateContext();
        context.Users.AddRange(first, second);
        await context.SaveChangesAsync();
        context.ExternalAccounts.Add(NewAccount(first.Id, providerUserId));
        await context.SaveChangesAsync();
        context.ExternalAccounts.Add(NewAccount(second.Id, providerUserId));

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [RequiresTestDatabaseFact]
    public async Task SaveChangesAsync_WhenTheProviderAccountWasRemoved_AllowsLinkingItAgain()
    {
        var providerUserId = $"{Guid.NewGuid():N}";
        var user = NewUser();
        await using var context = database.CreateContext();
        context.Users.Add(user);
        await context.SaveChangesAsync();
        var removed = NewAccount(user.Id, providerUserId);
        context.ExternalAccounts.Add(removed);
        await context.SaveChangesAsync();
        context.ExternalAccounts.Remove(removed);
        await context.SaveChangesAsync();

        context.ExternalAccounts.Add(NewAccount(user.Id, providerUserId));
        var written = await context.SaveChangesAsync();

        Assert.Equal(1, written);
    }

    [RequiresTestDatabaseFact]
    public async Task ExecuteSqlAsync_WhenTheUserHasAccountsAndItsRowIsDeleted_RejectsTheDeletion()
    {
        var user = NewUserWithAccount();
        await using var context = database.CreateContext();
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var delete = () => context.Database.ExecuteSqlAsync($"DELETE FROM Users WHERE Id = {user.Id}");

        await Assert.ThrowsAsync<SqlException>(delete);
    }
}
