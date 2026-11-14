using Life.Application.Common;
using Life.Domain.Users;
using Life.Infrastructure.Tests.Support;
using Life.Persistence;
using Life.Support;
using Microsoft.EntityFrameworkCore;

namespace Life.Infrastructure.Tests.Persistence;

[Collection(MigratedDatabaseCollection.Name)]
public class UnitOfWorkTests(MigratedDatabaseFixture database)
{
    private static User NewUser()
    {
        return new User { Email = $"{Guid.NewGuid():N}@life.test", Name = "Test user" };
    }

    [RequiresTestDatabaseFact]
    public async Task ExecuteInTransactionAsync_WhenTheOperationSucceeds_PersistsTheChanges()
    {
        var user = NewUser();
        await using var context = database.CreateContext();
        IUnitOfWork unitOfWork = new UnitOfWork(context);

        await unitOfWork.ExecuteInTransactionAsync(_ =>
        {
            context.Users.Add(user);

            return Task.CompletedTask;
        });

        await using var reader = database.CreateContext();
        Assert.True(await reader.Users.AnyAsync(candidate => candidate.Id == user.Id));
    }

    [RequiresTestDatabaseFact]
    public async Task ExecuteInTransactionAsync_WhenTheOperationFails_DiscardsTheChanges()
    {
        var user = NewUser();
        await using var context = database.CreateContext();
        IUnitOfWork unitOfWork = new UnitOfWork(context);

        await Assert.ThrowsAsync<InvalidOperationException>(() => unitOfWork.ExecuteInTransactionAsync(
            async token =>
            {
                context.Users.Add(user);
                await context.SaveChangesAsync(token);

                throw new InvalidOperationException("Operation failed.");
            }));

        await using var reader = database.CreateContext();
        Assert.False(await reader.Users.AnyAsync(candidate => candidate.Id == user.Id));
    }

    [RequiresTestDatabaseFact]
    public async Task ExecuteInTransactionAsync_WhenTheSaveViolatesAConstraint_DiscardsEveryChange()
    {
        var email = $"{Guid.NewGuid():N}@life.test";
        var other = NewUser();
        await using var context = database.CreateContext();
        IUnitOfWork unitOfWork = new UnitOfWork(context);
        context.Users.Add(new User { Email = email, Name = "Existing" });
        await context.SaveChangesAsync();

        await Assert.ThrowsAsync<DbUpdateException>(() => unitOfWork.ExecuteInTransactionAsync(_ =>
        {
            context.Users.AddRange(other, new User { Email = email, Name = "Duplicate" });

            return Task.CompletedTask;
        }));

        await using var reader = database.CreateContext();
        Assert.False(await reader.Users.AnyAsync(candidate => candidate.Id == other.Id));
    }
}
