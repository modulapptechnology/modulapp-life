using Application.Common;
using Domain.Users;

namespace Application.Users;

public sealed class RefreshSession(
    IRefreshTokenRepository refreshTokens,
    IVerificationTokenService tokenService,
    SessionIssuer sessionIssuer,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
{
    public async Task<LoginResponse?> ExecuteAsync(
        RefreshSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var tokenHash = tokenService.Hash(request.RefreshToken);

        return await unitOfWork.ExecuteInTransactionAsync(
            async innerToken =>
            {
                var now = timeProvider.GetUtcNow().UtcDateTime;
                var token = await refreshTokens.FindByHashAsync(tokenHash, innerToken);

                if (token?.User is null)
                {
                    return null;
                }

                if (token.WasUsed)
                {
                    await RevokeFamilyAsync(token.FamilyId, now, innerToken);

                    return null;
                }

                if (!token.CanBeUsed(now) || token.User.Status != UserStatus.Active)
                {
                    return null;
                }

                token.MarkUsed(now);

                return sessionIssuer.Issue(token.User, token.FamilyId, token.DeviceId, now);
            },
            cancellationToken);
    }

    private async Task RevokeFamilyAsync(Guid familyId, DateTime now, CancellationToken cancellationToken)
    {
        var family = await refreshTokens.ListFamilyAsync(familyId, cancellationToken);

        foreach (var member in family)
        {
            member.Revoke(now);
        }
    }
}
