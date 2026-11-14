using Life.Domain.Common;

namespace Life.Domain.Users;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;

    public string? PasswordHash { get; set; }

    public string Name { get; set; } = string.Empty;

    public UserStatus Status { get; set; } = UserStatus.Pending;

    public string? AvatarUrl { get; set; }

    public string? Locale { get; set; }

    public string? TimeZone { get; set; }

    public DateTime? EmailVerifiedAt { get; set; }

    public int FailedLoginAttempts { get; set; }

    public DateTime? LockedUntil { get; set; }

    public ICollection<ExternalAccount> ExternalAccounts { get; set; } = [];

    public bool IsLockedAt(DateTime now) => LockedUntil is not null && now < LockedUntil;

    public void RegisterFailedLogin(DateTime now, int maxFailedAttempts, TimeSpan lockoutDuration)
    {
        if (LockedUntil is not null && now >= LockedUntil)
        {
            FailedLoginAttempts = 0;
            LockedUntil = null;
        }

        FailedLoginAttempts++;

        if (FailedLoginAttempts >= maxFailedAttempts)
        {
            LockedUntil = now.Add(lockoutDuration);
        }
    }

    public void RegisterSuccessfulLogin()
    {
        FailedLoginAttempts = 0;
        LockedUntil = null;
    }

    public void VerifyEmail(DateTime verifiedAt)
    {
        EmailVerifiedAt = verifiedAt;

        if (Status == UserStatus.Pending)
        {
            Status = UserStatus.Active;
        }
    }
}
