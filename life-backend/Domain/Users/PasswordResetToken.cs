using Life.Domain.Common;

namespace Life.Domain.Users;

public class PasswordResetToken : BaseEntity
{
    public Guid UserId { get; set; }

    public User? User { get; set; }

    public string TokenHash { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public DateTime? UsedAt { get; set; }

    public bool CanBeUsed(DateTime now) => UsedAt is null && now < ExpiresAt;

    public void MarkUsed(DateTime usedAt)
    {
        if (!CanBeUsed(usedAt))
        {
            throw new InvalidOperationException("The password reset token is expired or already used.");
        }

        UsedAt = usedAt;
    }

    public void Invalidate(DateTime now)
    {
        UsedAt ??= now;
    }
}
