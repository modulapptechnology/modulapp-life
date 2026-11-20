using System.Net;
using System.Net.Mail;

namespace Life.Infrastructure.Email;

public sealed class SmtpEmailTransport(SmtpOptions smtp) : IEmailTransport
{
    public async Task SendAsync(
        string to,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(smtp.Host) || string.IsNullOrWhiteSpace(smtp.FromAddress))
        {
            throw new InvalidOperationException(
                $"The settings '{SmtpOptions.SectionName}:Host' and '{SmtpOptions.SectionName}:FromAddress' "
                    + "are required to send emails.");
        }

        using var message = new MailMessage(
            new MailAddress(smtp.FromAddress, smtp.FromName),
            new MailAddress(to))
        {
            Subject = subject,
            Body = body,
        };
        using var client = new SmtpClient(smtp.Host, smtp.Port) { EnableSsl = smtp.EnableSsl };

        if (!string.IsNullOrWhiteSpace(smtp.UserName))
        {
            client.Credentials = new NetworkCredential(smtp.UserName, smtp.Password);
        }

        await client.SendMailAsync(message, cancellationToken);
    }
}
