namespace Life.Infrastructure.Email;

public interface IEmailTransport
{
    Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
}
