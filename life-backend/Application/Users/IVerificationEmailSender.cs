namespace Application.Users;

public interface IVerificationEmailSender
{
    Task SendAsync(string email, string token, CancellationToken cancellationToken = default);
}
