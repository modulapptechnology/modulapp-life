using Application.Common;

namespace Application.Users;

public sealed class EndSession(
    IRefreshTokenRepository refreshTokens,
    IVerificationTokenService tokenService,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
{
    public async Task ExecuteAsync(string? refreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return;
        }

        var tokenHash = tokenService.Hash(refreshToken);

        await unitOfWork.ExecuteInTransactionAsync(
            async innerToken =>
            {
                var token = await refreshTokens.FindByHashAsync(tokenHash, innerToken);

                if (token is null)
                {
                    return true;
                }

                var now = timeProvider.GetUtcNow().UtcDateTime;
                var family = await refreshTokens.ListFamilyAsync(token.FamilyId, innerToken);

                foreach (var member in family)
                {
                    member.Revoke(now);
                }

                return true;
            },
            cancellationToken);
    }
}
