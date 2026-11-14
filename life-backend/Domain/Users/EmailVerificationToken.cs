using Life.Domain.Common;

namespace Life.Domain.Users;

public class EmailVerificationToken : BaseEntity
{
    public const int MaxFailedAttempts = 5;

    public Guid UserId { get; set; }

    public User? User { get; set; }

    public string TokenHash { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public DateTime? UsedAt { get; set; }

    public int FailedAttempts { get; set; }

    public bool CanBeUsed(DateTime now)
    {
        return UsedAt is null && now < ExpiresAt && FailedAttempts < MaxFailedAttempts;
    }

    public void RegisterFailedAttempt()
    {
        FailedAttempts++;
    }

    public void MarkUsed(DateTime usedAt)
    {
        if (!CanBeUsed(usedAt))
        {
            throw new InvalidOperationException(
                "The verification code is expired, already used or out of attempts.");
        }

        UsedAt = usedAt;
    }
}
