using System.Net;
using System.Net.Mail;
using Application.Users;

namespace Infrastructure.Email;

public sealed class SmtpVerificationEmailSender(
    SmtpOptions smtp,
    EmailVerificationOptions verification) : IVerificationEmailSender
{
    public async Task SendAsync(string email, string code, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(smtp.Host) || string.IsNullOrWhiteSpace(smtp.FromAddress))
        {
            throw new InvalidOperationException(
                $"The settings '{SmtpOptions.SectionName}:Host' and '{SmtpOptions.SectionName}:FromAddress' "
                    + "are required to send emails.");
        }

        using var message = new MailMessage(smtp.FromAddress, email)
        {
            Subject = VerificationEmailContent.Subject,
            Body = VerificationEmailContent.BuildBody(code, verification.TokenLifetime),
        };
        using var client = new SmtpClient(smtp.Host, smtp.Port) { EnableSsl = smtp.EnableSsl };

        if (!string.IsNullOrWhiteSpace(smtp.UserName))
        {
            client.Credentials = new NetworkCredential(smtp.UserName, smtp.Password);
        }

        await client.SendMailAsync(message, cancellationToken);
    }
}
