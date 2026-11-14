using Life.Application.Users;
using Life.Domain.Users;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Life.Persistence;

public sealed class UserRepository(LifeDbContext context) : IUserRepository
{
    private const int _duplicateKeyIndexError = 2601;

    private const int _duplicateKeyConstraintError = 2627;

    public async Task AddAsync(
        User user,
        EmailVerificationToken token,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(token);

        context.Users.Add(user);
        context.EmailVerificationTokens.Add(token);

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsDuplicateKey(exception))
        {
            throw new DuplicateEmailException(exception);
        }
    }

    public Task<EmailVerificationToken?> FindLatestVerificationTokenAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(email);

        return context.EmailVerificationTokens
            .Include(token => token.User)
            .Where(token => token.User!.Email == email)
            .OrderByDescending(token => token.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(email);

        return context.Users.FirstOrDefaultAsync(user => user.Email == email, cancellationToken);
    }

    public void AddLoginAttempt(LoginAttempt attempt)
    {
        ArgumentNullException.ThrowIfNull(attempt);

        context.LoginAttempts.Add(attempt);
    }

    private static bool IsDuplicateKey(DbUpdateException exception)
    {
        return exception.InnerException is SqlException
        {
            Number: _duplicateKeyIndexError or _duplicateKeyConstraintError,
        };
    }
}
