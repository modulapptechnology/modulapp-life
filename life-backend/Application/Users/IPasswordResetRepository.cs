using Life.Domain.Users;

namespace Life.Application.Users;

public interface IPasswordResetRepository
{
    void Add(PasswordResetToken token);

    Task<PasswordResetToken?> FindByHashAsync(string tokenHash, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PasswordResetToken>> ListPendingAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
