using System.Security.Cryptography;
using System.Text;
using Life.Application.Common;

namespace Life.Application.Users;

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

        var email = request.Email.Trim();
        var codeHash = tokenService.Hash(request.Code);

        return await unitOfWork.ExecuteInTransactionAsync(
            async innerToken =>
            {
                var token = await users.FindLatestVerificationTokenAsync(email, innerToken);
                var now = timeProvider.GetUtcNow().UtcDateTime;

                if (token?.User is null || !token.CanBeUsed(now))
                {
                    return false;
                }

                if (!HashesMatch(token.TokenHash, codeHash))
                {
                    token.RegisterFailedAttempt();

                    return false;
                }

                token.MarkUsed(now);
                token.User.VerifyEmail(now);

                return true;
            },
            cancellationToken);
    }

    private static bool HashesMatch(string storedHash, string candidateHash)
    {
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(storedHash),
            Encoding.UTF8.GetBytes(candidateHash));
    }
}
