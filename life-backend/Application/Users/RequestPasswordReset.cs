using System.Globalization;
using Life.Application.Common;
using Life.Application.Email;
using Life.Domain.Users;
using Microsoft.Extensions.Logging;

namespace Life.Application.Users;

public sealed class RequestPasswordReset(
    IUserRepository users,
    IPasswordResetRepository resets,
    IVerificationTokenService tokenService,
    IEmailSender emailSender,
    IUnitOfWork unitOfWork,
    PasswordRecoveryOptions options,
    TimeProvider timeProvider,
    ILogger<RequestPasswordReset> logger)
{
    public async Task ExecuteAsync(
        RequestPasswordResetRequest request,
        string? locale = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var user = await users.FindByEmailAsync(request.Email.Trim(), cancellationToken);

        if (user is null || user.Status == UserStatus.Blocked)
        {
            return;
        }

        var token = tokenService.Create();

        await unitOfWork.ExecuteInTransactionAsync(
            async innerToken =>
            {
                var now = timeProvider.GetUtcNow().UtcDateTime;

                foreach (var pending in await resets.ListPendingAsync(user.Id, innerToken))
                {
                    pending.Invalidate(now);
                }

                resets.Add(new PasswordResetToken
                {
                    UserId = user.Id,
                    TokenHash = token.Hash,
                    ExpiresAt = now.Add(options.TokenLifetime),
                });

                return true;
            },
            cancellationToken);

        await SendEmailAsync(user, token, locale, cancellationToken);
    }

    private async Task SendEmailAsync(
        User user,
        VerificationToken token,
        string? locale,
        CancellationToken cancellationToken)
    {
        try
        {
            var minutes = (int)Math.Ceiling(options.TokenLifetime.TotalMinutes);
            var separator = options.ResetUrl.Contains('?', StringComparison.Ordinal) ? '&' : '?';
            var values = new Dictionary<string, string>
            {
                [EmailValueKeys.Link] =
                    $"{options.ResetUrl}{separator}{PasswordRecoveryOptions.TokenQueryName}={token.Value}",
                [EmailValueKeys.Minutes] = minutes.ToString(CultureInfo.InvariantCulture),
            };

            await emailSender.SendAsync(
                new EmailMessage(user.Email, EmailTemplate.PasswordReset, values, user.Locale ?? locale),
                cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(exception, "The password reset email for user {UserId} could not be sent.", user.Id);
        }
    }
}
