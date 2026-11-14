using Life.Domain.Users;
using Life.Infrastructure.Tests.Support;
using Life.Persistence.Configuration;
using Life.Support;
using Microsoft.EntityFrameworkCore;

namespace Life.Infrastructure.Tests.Persistence;

[Collection(MigratedDatabaseCollection.Name)]
public class UserPersistenceTests(MigratedDatabaseFixture database)
{
    private static User NewUser(string? email = null)
    {
        return new User { Email = email ?? $"{Guid.NewGuid():N}@life.test", Name = "Test user" };
    }

    [RequiresTestDatabaseFact]
    public async Task SaveChangesAsync_WhenAUserIsAdded_StoresItWithGeneratedIdAndAuditDates()
    {
        var user = NewUser();
        await using var writer = database.CreateContext();
        writer.Users.Add(user);
        await writer.SaveChangesAsync();

        await using var reader = database.CreateContext();
        var stored = await reader.Users.AsNoTracking().SingleAsync(candidate => candidate.Id == user.Id);

        Assert.Equal(
            (user.Email, UserStatus.Pending, false, true),
            (stored.Email, stored.Status, stored.IsDeleted, stored.CreatedAt == stored.UpdatedAt));
    }

    [RequiresTestDatabaseFact]
    public async Task SaveChangesAsync_WhenAUserIsModified_KeepsCreationAndChangesTheRowVersion()
    {
        var user = NewUser();
        await using var writer = database.CreateContext();
        writer.Users.Add(user);
        await writer.SaveChangesAsync();
        var createdAt = user.CreatedAt;
        var version = user.RowVersion.ToArray();

        user.Name = "Renamed";
        await writer.SaveChangesAsync();

        await using var reader = database.CreateContext();
        var stored = await reader.Users.AsNoTracking().SingleAsync(candidate => candidate.Id == user.Id);
        var versionChanged = !stored.RowVersion.SequenceEqual(version);
        Assert.Equal(
            ("Renamed", createdAt, true, true),
            (stored.Name, stored.CreatedAt, stored.UpdatedAt >= createdAt, versionChanged));
    }

    [RequiresTestDatabaseFact]
    public async Task SaveChangesAsync_WhenAUserIsRemoved_HidesItButKeepsTheRowMarkedAsDeleted()
    {
        var user = NewUser();
        await using var writer = database.CreateContext();
        writer.Users.Add(user);
        await writer.SaveChangesAsync();

        writer.Users.Remove(user);
        await writer.SaveChangesAsync();

        await using var reader = database.CreateContext();
        var visible = await reader.Users.CountAsync(candidate => candidate.Id == user.Id);
        var stored = await reader.Users.IgnoreQueryFilters()
            .SingleAsync(candidate => candidate.Id == user.Id);
        Assert.Equal((0, true, true), (visible, stored.IsDeleted, stored.DeletedAt is not null));
    }

    [RequiresTestDatabaseFact]
    public async Task SaveChangesAsync_WhenTheRowVersionIsStale_ThrowsAConcurrencyException()
    {
        var user = NewUser();
        await using var first = database.CreateContext();
        first.Users.Add(user);
        await first.SaveChangesAsync();
        await using var second = database.CreateContext();
        var sameUser = await second.Users.SingleAsync(candidate => candidate.Id == user.Id);
        user.Name = "First change";
        await first.SaveChangesAsync();

        sameUser.Name = "Second change";

        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => second.SaveChangesAsync());
    }

    [RequiresTestDatabaseFact]
    public async Task SaveChangesAsync_WhenTheEmailDiffersOnlyInCase_RejectsTheDuplicate()
    {
        var email = $"{Guid.NewGuid():N}@life.test";
        await using var context = database.CreateContext();
        context.Users.Add(NewUser(email));
        await context.SaveChangesAsync();
        context.Users.Add(NewUser(email.ToUpperInvariant()));

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [RequiresTestDatabaseFact]
    public async Task SaveChangesAsync_WhenTheEmailBelongsToARemovedUser_AllowsRegisteringItAgain()
    {
        var email = $"{Guid.NewGuid():N}@life.test";
        await using var context = database.CreateContext();
        var removed = NewUser(email);
        context.Users.Add(removed);
        await context.SaveChangesAsync();
        context.Users.Remove(removed);
        await context.SaveChangesAsync();

        context.Users.Add(NewUser(email));
        await context.SaveChangesAsync();

        var activeUsers = await context.Users.CountAsync(candidate => candidate.Email == email);
        Assert.Equal(1, activeUsers);
    }

    [RequiresTestDatabaseFact]
    public async Task SaveChangesAsync_WhenTheEmailExceedsTheMaximumLength_RejectsTheUser()
    {
        var tooLong = new string('a', UserConfiguration.EmailMaxLength + 1);
        await using var context = database.CreateContext();
        context.Users.Add(NewUser(tooLong));

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [RequiresTestDatabaseFact]
    public async Task SaveChangesAsync_WhenTheStatusIsNotAKnownValue_RejectsTheUser()
    {
        var user = NewUser();
        user.Status = (UserStatus)99;
        await using var context = database.CreateContext();
        context.Users.Add(user);

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [RequiresTestDatabaseFact]
    public async Task SaveChangesAsync_WhenTheStatusIsAKnownValue_StoresTheUser()
    {
        var user = NewUser();
        user.Status = UserStatus.Blocked;
        await using var context = database.CreateContext();
        context.Users.Add(user);

        var written = await context.SaveChangesAsync();

        Assert.Equal(1, written);
    }

    [RequiresTestDatabaseFact]
    public async Task SaveChangesAsync_WhenTheEmailHasTheMaximumLength_StoresTheUser()
    {
        var email = $"{Guid.NewGuid():N}" + new string('a', UserConfiguration.EmailMaxLength - 32);
        await using var context = database.CreateContext();
        context.Users.Add(NewUser(email));

        var written = await context.SaveChangesAsync();

        Assert.Equal(1, written);
    }
}
