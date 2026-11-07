using Domain.Common;

namespace Domain.Users;

public class EmailVerificationToken : BaseEntity
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
            throw new InvalidOperationException("The verification token is expired or already used.");
        }

        UsedAt = usedAt;
    }
}
