using Microsoft.Extensions.Logging;

namespace Life.Infrastructure.Email;

public sealed class LoggingEmailTransport(ILogger<LoggingEmailTransport> logger) : IEmailTransport
{
    public Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Email to {Recipient} was not delivered because the Log provider only records the send.",
            EmailAddressMask.Mask(to));

        return Task.CompletedTask;
    }
}
