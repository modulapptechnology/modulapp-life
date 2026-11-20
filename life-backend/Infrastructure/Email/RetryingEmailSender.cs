using Life.Application.Email;
using Microsoft.Extensions.Logging;

namespace Life.Infrastructure.Email;

public sealed class RetryingEmailSender(
    EmailTemplateRenderer renderer,
    IEmailTransport transport,
    EmailRetryOptions retry,
    TimeProvider timeProvider,
    ILogger<RetryingEmailSender> logger) : IEmailSender
{
    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        var rendered = renderer.Render(message);

        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await transport.SendAsync(message.To, rendered.Subject, rendered.Body, cancellationToken);

                return;
            }
            catch (Exception exception) when (IsTransient(exception))
            {
                if (attempt >= retry.MaxAttempts)
                {
                    logger.LogError(
                        "The {Template} email to {Recipient} failed after {Attempts} attempts ({ExceptionType}).",
                        message.Template,
                        EmailAddressMask.Mask(message.To),
                        attempt,
                        exception.GetType().Name);

                    throw;
                }

                var delay = retry.InitialDelay * Math.Pow(2, attempt - 1);

                logger.LogWarning(
                    "Attempt {Attempt} of {MaxAttempts} of the {Template} email to {Recipient} failed "
                        + "({ExceptionType}); retrying in {Delay}.",
                    attempt,
                    retry.MaxAttempts,
                    message.Template,
                    EmailAddressMask.Mask(message.To),
                    exception.GetType().Name,
                    delay);

                await Task.Delay(delay, timeProvider, cancellationToken);
            }
        }
    }

    private static bool IsTransient(Exception exception)
    {
        return exception is not (OperationCanceledException or InvalidOperationException);
    }
}
