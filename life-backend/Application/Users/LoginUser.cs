using Application.Common;
using Domain.Users;

namespace Application.Users;

public sealed class LoginUser(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    SessionIssuer sessionIssuer,
    IUnitOfWork unitOfWork,
    LoginOptions options,
    TimeProvider timeProvider)
{
    public async Task<LoginResponse?> ExecuteAsync(
        LoginRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var email = request.Email.Trim();

        return await unitOfWork.ExecuteInTransactionAsync(
            async innerToken =>
            {
                var now = timeProvider.GetUtcNow().UtcDateTime;
                var user = await users.FindByEmailAsync(email, innerToken);
                var passwordMatches = passwordHasher.Verify(request.Password, user?.PasswordHash);
                var outcome = Evaluate(user, passwordMatches, now);

                users.AddLoginAttempt(new LoginAttempt
                {
                    UserId = user?.Id,
                    Email = email,
                    IpAddress = ipAddress,
                    Outcome = outcome,
                    OccurredAt = now,
                });

                if (outcome != LoginOutcome.Succeeded)
                {
                    return null;
                }

                var deviceId = string.IsNullOrWhiteSpace(request.DeviceId)
                    ? Guid.NewGuid().ToString("N")
                    : request.DeviceId.Trim();

                return sessionIssuer.Issue(user!, Guid.NewGuid(), deviceId, now);
            },
            cancellationToken);
    }

    private LoginOutcome Evaluate(User? user, bool passwordMatches, DateTime now)
    {
        if (user is null)
        {
            return LoginOutcome.InvalidCredentials;
        }

        if (user.IsLockedAt(now))
        {
            return LoginOutcome.AccountLocked;
        }

        if (!passwordMatches)
        {
            user.RegisterFailedLogin(now, options.MaxFailedAttempts, options.LockoutDuration);

            return LoginOutcome.InvalidCredentials;
        }

        if (user.Status != UserStatus.Active)
        {
            return LoginOutcome.AccountInactive;
        }

        user.RegisterSuccessfulLogin();

        return LoginOutcome.Succeeded;
    }
}
