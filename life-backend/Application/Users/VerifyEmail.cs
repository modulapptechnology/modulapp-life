using Application.Common;

namespace Application.Users;

public sealed class VerifyEmail(
    IUserRepository users,
    IVerificationTokenService tokenService,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
{
    public async Task<bool> ExecuteAsync(
        VerifyEmailRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var tokenHash = tokenService.Hash(request.Token);

        return await unitOfWork.ExecuteInTransactionAsync(
            async innerToken =>
            {
                var token = await users.FindVerificationTokenAsync(tokenHash, innerToken);
                var now = timeProvider.GetUtcNow().UtcDateTime;

                if (token?.User is null || !token.CanBeUsed(now))
                {
                    return false;
                }

                token.MarkUsed(now);
                token.User.VerifyEmail(now);

                return true;
            },
            cancellationToken);
    }
}
