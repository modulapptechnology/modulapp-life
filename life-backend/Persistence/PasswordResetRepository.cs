using Life.Application.Users;
using Life.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Life.Persistence;

public sealed class PasswordResetRepository(LifeDbContext context) : IPasswordResetRepository
{
    public void Add(PasswordResetToken token)
    {
        ArgumentNullException.ThrowIfNull(token);

        context.PasswordResetTokens.Add(token);
    }

    public Task<PasswordResetToken?> FindByHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tokenHash);

        return context.PasswordResetTokens
            .Include(token => token.User)
            .FirstOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);
    }

    public async Task<IReadOnlyList<PasswordResetToken>> ListPendingAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await context.PasswordResetTokens
            .Where(token => token.UserId == userId && token.UsedAt == null)
            .ToListAsync(cancellationToken);
    }
}
