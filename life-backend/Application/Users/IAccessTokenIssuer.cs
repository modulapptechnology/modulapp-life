using Domain.Users;

namespace Application.Users;

public interface IAccessTokenIssuer
{
    AccessToken Issue(User user, DateTime issuedAt);
}
