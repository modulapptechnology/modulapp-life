using Life.Application.Common;
using Life.Application.Email;
using Life.Domain.Users;
using Microsoft.Extensions.Logging;

namespace Life.Application.Users;

public sealed class ChangePassword(
    IUserRepository users,
    IRefreshTokenRepository refreshTokens,
    IPasswordHasher passwordHasher,
    IEmailSender emailSender,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    ILogger<ChangePassword> logger)
{
    public async Task<ChangePasswordOutcome> ExecuteAsync(
        Guid userId,
        Guid? currentSessionId,
        ChangePasswordRequest request,
        string? locale = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        User? changedUser = null;

        var outcome = await unitOfWork.ExecuteInTransactionAsync(
            async innerToken =>
            {
                var user = await users.FindByIdAsync(userId, innerToken);

                if (user is null || user.Status != UserStatus.Active)
                {
                    return ChangePasswordOutcome.AccountUnavailable;
                }

                if (!passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
                {
                    return ChangePasswordOutcome.CurrentPasswordIncorrect;
                }

                if (passwordHasher.Verify(request.NewPassword, user.PasswordHash))
                {
                    return ChangePasswordOutcome.NewPasswordSameAsCurrent;
                }

                var now = timeProvider.GetUtcNow().UtcDateTime;
                user.ChangePassword(passwordHasher.Hash(request.NewPassword));

                foreach (var session in await refreshTokens.ListActiveByUserAsync(userId, innerToken))
                {
                    if (session.FamilyId != currentSessionId)
                    {
                        session.Revoke(now);
                    }
                }

                changedUser = user;

                return ChangePasswordOutcome.Changed;
            },
            cancellationToken);

        if (changedUser is not null)
        {
            await SendNoticeAsync(changedUser, locale, cancellationToken);
        }

        return outcome;
    }

    private async Task SendNoticeAsync(User user, string? locale, CancellationToken cancellationToken)
    {
        try
        {
            await emailSender.SendAsync(
                new EmailMessage(
                    user.Email,
                    EmailTemplate.PasswordChanged,
                    new Dictionary<string, string>(),
                    user.Locale ?? locale),
                cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(exception, "The password change notice for user {UserId} could not be sent.", user.Id);
        }
    }
}
