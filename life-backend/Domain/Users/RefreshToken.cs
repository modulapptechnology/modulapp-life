using Life.Domain.Common;

namespace Life.Domain.Users;

public class RefreshToken : BaseEntity
{
    public Guid UserId { get; set; }

    public User? User { get; set; }

    public Guid FamilyId { get; set; }

    public string DeviceId { get; set; } = string.Empty;

    public string TokenHash { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public DateTime? UsedAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    public bool WasUsed => UsedAt is not null;

    public bool CanBeUsed(DateTime now) => UsedAt is null && RevokedAt is null && now < ExpiresAt;

    public void MarkUsed(DateTime usedAt)
    {
        if (!CanBeUsed(usedAt))
        {
            throw new InvalidOperationException("The refresh token is expired, revoked or already used.");
        }

        UsedAt = usedAt;
    }

    public void Revoke(DateTime revokedAt)
    {
        RevokedAt ??= revokedAt;
    }
}
