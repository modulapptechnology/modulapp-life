using Application.Users;
using Domain.Users;
using Infrastructure.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Support;

namespace Infrastructure.Tests.Persistence;

[Collection(MigratedDatabaseCollection.Name)]
public class UserRepositoryTests(MigratedDatabaseFixture database)
{
    private static User NewUser(string? email = null)
    {
        return new User { Email = email ?? $"{Guid.NewGuid():N}@life.test" };
    }

    private static EmailVerificationToken NewToken(User user, string? hash = null)
    {
        return new EmailVerificationToken
        {
            User = user,
            TokenHash = hash ?? Guid.NewGuid().ToString("N"),
            ExpiresAt = DateTime.UtcNow.AddHours(1),
        };
    }

    [RequiresTestDatabaseFact]
    public async Task AddAsync_WithNewUser_StoresTheUserAndItsTokenHash()
    {
        var user = NewUser();
        var token = NewToken(user, "stored-hash-" + Guid.NewGuid().ToString("N"));
        await using var context = database.CreateContext();
        var repository = new UserRepository(context);

        await repository.AddAsync(user, token);

        await using var reader = database.CreateContext();
        var stored = await reader.EmailVerificationTokens.AsNoTracking()
            .SingleAsync(candidate => candidate.UserId == user.Id);
        Assert.Equal((token.TokenHash, false), (stored.TokenHash, stored.UsedAt.HasValue));
    }

    [RequiresTestDatabaseFact]
    public async Task AddAsync_WithAnEmailThatDiffersOnlyInCase_ThrowsDuplicateEmailException()
    {
        var email = $"{Guid.NewGuid():N}@life.test";
        var original = NewUser(email);
        await using var first = database.CreateContext();
        await new UserRepository(first).AddAsync(original, NewToken(original));
        await using var second = database.CreateContext();
        var repository = new UserRepository(second);
        var duplicate = NewUser(email.ToUpperInvariant());

        var action = () => repository.AddAsync(duplicate, NewToken(duplicate));

        await Assert.ThrowsAsync<DuplicateEmailException>(action);
    }

    [RequiresTestDatabaseFact]
    public async Task FindVerificationTokenAsync_WithKnownHash_ReturnsTheTokenWithItsUser()
    {
        var user = NewUser();
        var token = NewToken(user);
        await using var writer = database.CreateContext();
        await new UserRepository(writer).AddAsync(user, token);
        await using var context = database.CreateContext();
        var repository = new UserRepository(context);

        var found = await repository.FindVerificationTokenAsync(token.TokenHash);

        Assert.Equal(user.Email, found?.User?.Email);
    }

    [RequiresTestDatabaseFact]
    public async Task FindVerificationTokenAsync_WithUnknownHash_ReturnsNull()
    {
        await using var context = database.CreateContext();
        var repository = new UserRepository(context);

        var found = await repository.FindVerificationTokenAsync("unknown-hash");

        Assert.Null(found);
    }
}
