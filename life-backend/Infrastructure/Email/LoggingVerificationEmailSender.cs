using Application.Users;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Email;

public sealed class LoggingVerificationEmailSender(
    EmailVerificationOptions verification,
    ILogger<LoggingVerificationEmailSender> logger) : IVerificationEmailSender
{
    public Task SendAsync(string email, string token, CancellationToken cancellationToken = default)
    {
        var link = VerificationEmailContent.BuildLink(verification.VerificationUrl, token);

        logger.LogInformation("Verification email for {Email}: {Link}", email, link);

        return Task.CompletedTask;
    }
}
