using Life.Domain.Common;

namespace Life.Domain.Users;

public class ExternalAccount : BaseEntity
{
    public Guid UserId { get; set; }

    public User? User { get; set; }

    public string Provider { get; set; } = string.Empty;

    public string ProviderUserId { get; set; } = string.Empty;
}
