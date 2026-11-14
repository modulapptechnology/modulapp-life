using Life.Domain.Users;

namespace Life.Application.Users;

public interface IAccessTokenIssuer
{
    AccessToken Issue(User user, DateTime issuedAt);
}
