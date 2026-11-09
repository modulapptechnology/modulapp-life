namespace Application.Users;

public interface IVerificationEmailSender
{
    Task SendAsync(string email, string code, CancellationToken cancellationToken = default);
}
