using Life.Application.Common;
using Life.Domain.Users;

namespace Life.Application.Users;

public sealed class ResetPassword(
    IPasswordResetRepository resets,
    IRefreshTokenRepository refreshTokens,
    IPasswordHasher passwordHasher,
    IVerificationTokenService tokenService,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
{
    public async Task<bool> ExecuteAsync(
        ResetPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var tokenHash = tokenService.Hash(request.Token);

        return await unitOfWork.ExecuteInTransactionAsync(
            async innerToken =>
            {
                var token = await resets.FindByHashAsync(tokenHash, innerToken);
                var now = timeProvider.GetUtcNow().UtcDateTime;

                if (token?.User is null || token.User.Status == UserStatus.Blocked || !token.CanBeUsed(now))
                {
                    return false;
                }

                token.MarkUsed(now);
                token.User.ResetPassword(passwordHasher.Hash(request.NewPassword), now);

                foreach (var session in await refreshTokens.ListActiveByUserAsync(token.UserId, innerToken))
                {
                    session.Revoke(now);
                }

                return true;
            },
            cancellationToken);
    }
}
