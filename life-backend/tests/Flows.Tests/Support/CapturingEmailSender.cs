using System.Collections.Concurrent;
using Application.Users;

namespace Flows.Tests.Support;

public sealed class CapturingEmailSender : IVerificationEmailSender
{
    private readonly ConcurrentQueue<(string Email, string Code)> _sent = new();

    public Task SendAsync(string email, string code, CancellationToken cancellationToken = default)
    {
        _sent.Enqueue((email, code));

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
