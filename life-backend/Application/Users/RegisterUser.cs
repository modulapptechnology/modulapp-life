using Application.Common;
using Domain.Users;
using Microsoft.Extensions.Logging;

namespace Application.Users;

public sealed class RegisterUser(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    IVerificationTokenService tokenService,
    IVerificationEmailSender emailSender,
    IUnitOfWork unitOfWork,
    EmailVerificationOptions options,
    TimeProvider timeProvider,
    ILogger<RegisterUser> logger)
{
    public async Task<RegisterUserResponse?> ExecuteAsync(
        RegisterUserRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var token = tokenService.Create();
        var user = new User
        {
            Email = request.Email.Trim(),
            PasswordHash = passwordHasher.Hash(request.Password),
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
            await emailSender.SendAsync(user.Email, token.Value, cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(exception, "The verification email for user {UserId} could not be sent.", user.Id);
        }
    }
}
