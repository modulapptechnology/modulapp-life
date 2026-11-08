using Domain.Users;

namespace Application.Users;

public interface IRefreshTokenRepository
{
    void Add(RefreshToken token);

    Task<RefreshToken?> FindByHashAsync(string tokenHash, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RefreshToken>> ListFamilyAsync(Guid familyId, CancellationToken cancellationToken = default);
}
