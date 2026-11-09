using Application.Users;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Email;

public sealed class LoggingVerificationEmailSender(
    ILogger<LoggingVerificationEmailSender> logger) : IVerificationEmailSender
{
    public Task SendAsync(string email, string code, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Verification code for {Email}: {Code}", email, code);

        return Task.CompletedTask;
    }
}
