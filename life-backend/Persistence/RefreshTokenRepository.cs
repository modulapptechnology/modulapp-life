using Application.Users;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Persistence;

public sealed class RefreshTokenRepository(LifeDbContext context) : IRefreshTokenRepository
{
    public void Add(RefreshToken token)
    {
        ArgumentNullException.ThrowIfNull(token);

        context.RefreshTokens.Add(token);
    }

    public Task<RefreshToken?> FindByHashAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tokenHash);

        return context.RefreshTokens
            .Include(token => token.User)
            .FirstOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);
    }

    public async Task<IReadOnlyList<RefreshToken>> ListFamilyAsync(
        Guid familyId,
        CancellationToken cancellationToken = default)
    {
        return await context.RefreshTokens
            .Where(token => token.FamilyId == familyId)
            .ToListAsync(cancellationToken);
    }
}
