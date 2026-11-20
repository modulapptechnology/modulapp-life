using Life.Domain.Users;

namespace Life.Application.Users;

public interface IAccessTokenIssuer
{
    AccessToken Issue(User user, Guid sessionId, DateTime issuedAt);
}
