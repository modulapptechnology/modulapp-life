using System.Globalization;
using Life.Application.Common;
using Life.Application.Email;
using Life.Domain.Users;
using Microsoft.Extensions.Logging;

namespace Life.Application.Users;

public sealed class RegisterUser(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    IVerificationTokenService tokenService,
    IEmailSender emailSender,
    IUnitOfWork unitOfWork,
    EmailVerificationOptions options,
    TimeProvider timeProvider,
    ILogger<RegisterUser> logger)
{
    public async Task<RegisterUserResponse?> ExecuteAsync(
        RegisterUserRequest request,
        string? locale = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var token = tokenService.CreateCode();
        var user = new User
        {
            Email = request.Email.Trim(),
            PasswordHash = passwordHasher.Hash(request.Password),
            Locale = locale,
        };
        var verification = new EmailVerificationToken
        {
            User = user,
            TokenHash = token.Hash,
            ExpiresAt = timeProvider.GetUtcNow().UtcDateTime.Add(options.TokenLifetime),
        };

        try
        {
            await unitOfWork.ExecuteInTransactionAsync(
                innerToken => users.AddAsync(user, verification, innerToken),
                cancellationToken);
        }
        catch (DuplicateEmailException)
        {
            return null;
        }

        await SendVerificationEmailAsync(user, token, cancellationToken);

        return new RegisterUserResponse(user.Id, user.Email);
    }

    private async Task SendVerificationEmailAsync(
        User user,
        VerificationToken token,
        CancellationToken cancellationToken)
    {
        try
        {
            var minutes = (int)Math.Ceiling(options.TokenLifetime.TotalMinutes);
            var values = new Dictionary<string, string>
            {
                [EmailValueKeys.Code] = token.Value,
                [EmailValueKeys.Minutes] = minutes.ToString(CultureInfo.InvariantCulture),
            };

            await emailSender.SendAsync(
                new EmailMessage(user.Email, EmailTemplate.EmailVerification, values, user.Locale),
                cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(exception, "The verification email for user {UserId} could not be sent.", user.Id);
        }
    }
}
