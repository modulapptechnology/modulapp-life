using Domain.Users;

namespace Application.Users;

public interface IUserRepository
{
    Task AddAsync(User user, EmailVerificationToken token, CancellationToken cancellationToken = default);

    Task<EmailVerificationToken?> FindVerificationTokenAsync(
        string tokenHash,
        CancellationToken cancellationToken = default);
}
