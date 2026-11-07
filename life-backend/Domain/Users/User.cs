using Domain.Common;

namespace Domain.Users;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;

    public string? PasswordHash { get; set; }

    public string Name { get; set; } = string.Empty;

    public UserStatus Status { get; set; } = UserStatus.Pending;

    public string? AvatarUrl { get; set; }

    public string? Locale { get; set; }

    public string? TimeZone { get; set; }

    public ICollection<ExternalAccount> ExternalAccounts { get; set; } = [];
}
