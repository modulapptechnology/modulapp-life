using Life.Infrastructure.Email;

namespace Life.Infrastructure.Tests.Email;

public sealed class FakeEmailTransport(params Exception?[] outcomes) : IEmailTransport
{
    private readonly Queue<Exception?> _outcomes = new(outcomes);

    public List<(string To, string Subject, string Body)> Sent { get; } = [];

    public int Attempts { get; private set; }

    public Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        Attempts++;

        if (_outcomes.Count > 0 && _outcomes.Dequeue() is { } failure)
        {
            return Task.FromException(failure);
        }

        Sent.Add((to, subject, body));

        return Task.CompletedTask;
    }
}
