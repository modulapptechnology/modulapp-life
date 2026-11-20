using System.Collections.Concurrent;
using Life.Application.Email;
using Life.Application.Users;

namespace Life.Flows.Tests.Support;

public sealed class CapturingEmailSender : IEmailSender
{
    private readonly ConcurrentQueue<(string Email, string Code)> _sent = new();

    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        _sent.Enqueue((message.To, message.Values[EmailValueKeys.Code]));

        return Task.CompletedTask;
    }

    public string? LastCodeFor(string email)
    {
        return _sent
            .Where(item => string.Equals(item.Email, email, StringComparison.OrdinalIgnoreCase))
            .Select(item => item.Code)
            .LastOrDefault();
    }
}
