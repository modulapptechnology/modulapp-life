using Life.Domain.Users;
using Life.Infrastructure.Tests.Support;
using Life.Persistence;
using Life.Support;
using Microsoft.EntityFrameworkCore;

namespace Life.Infrastructure.Tests.Persistence;

[Collection(MigratedDatabaseCollection.Name)]
public class RefreshTokenRepositoryTests(MigratedDatabaseFixture database)
{
    private async Task<User> StoreUserAsync()
    {
        var user = new User { Email = $"{Guid.NewGuid():N}@life.test" };
        var verification = new EmailVerificationToken
        {
            User = user,
            TokenHash = Guid.NewGuid().ToString("N"),
            ExpiresAt = DateTime.UtcNow.AddHours(1),
        };
        await using var context = database.CreateContext();
        await new UserRepository(context).AddAsync(user, verification);

        return user;
    }

    private async Task<RefreshToken> StoreTokenAsync(User user, Guid familyId)
    {
        var token = new RefreshToken
        {
            UserId = user.Id,
            FamilyId = familyId,
            DeviceId = "phone-1",
            TokenHash = Guid.NewGuid().ToString("N"),
            ExpiresAt = DateTime.UtcNow.AddDays(60),
        };
        await using var context = database.CreateContext();
        new RefreshTokenRepository(context).Add(token);
        await context.SaveChangesAsync();

        return token;
    }

    [RequiresTestDatabaseFact]
    public async Task FindByHashAsync_WithKnownHash_ReturnsTheTokenWithItsUser()
    {
        var user = await StoreUserAsync();
        var stored = await StoreTokenAsync(user, Guid.NewGuid());
        await using var context = database.CreateContext();
        var repository = new RefreshTokenRepository(context);

        var found = await repository.FindByHashAsync(stored.TokenHash);

        Assert.Equal((user.Id, "phone-1"), (found?.User?.Id, found?.DeviceId));
    }

    [RequiresTestDatabaseFact]
    public async Task FindByHashAsync_WithUnknownHash_ReturnsNull()
    {
        await using var context = database.CreateContext();
        var repository = new RefreshTokenRepository(context);

        var found = await repository.FindByHashAsync("unknown-hash");

        Assert.Null(found);
    }

    [RequiresTestDatabaseFact]
    public async Task ListFamilyAsync_WithAFamily_ReturnsOnlyItsTokens()
    {
        var user = await StoreUserAsync();
        var familyId = Guid.NewGuid();
        await StoreTokenAsync(user, familyId);
        await StoreTokenAsync(user, familyId);
        await StoreTokenAsync(user, Guid.NewGuid());
        await using var context = database.CreateContext();
        var repository = new RefreshTokenRepository(context);

        var family = await repository.ListFamilyAsync(familyId);

        Assert.Equal(2, family.Count);
    }

    [RequiresTestDatabaseFact]
    public async Task SaveChanges_WithRevokedFamily_PersistsTheRevocation()
    {
        var user = await StoreUserAsync();
        var familyId = Guid.NewGuid();
        var stored = await StoreTokenAsync(user, familyId);
        await using var context = database.CreateContext();
        var repository = new RefreshTokenRepository(context);
        var family = await repository.ListFamilyAsync(familyId);
        family.Single().Revoke(DateTime.UtcNow);

        await context.SaveChangesAsync();

        await using var reader = database.CreateContext();
        var reloaded = await reader.RefreshTokens.AsNoTracking().SingleAsync(token => token.Id == stored.Id);
        Assert.NotNull(reloaded.RevokedAt);
    }
}
