using Domain.Common;

namespace Domain.Users;

public class LoginAttempt : BaseEntity
{
    public Guid? UserId { get; set; }

    public User? User { get; set; }

    public string Email { get; set; } = string.Empty;

    public string? IpAddress { get; set; }

    public LoginOutcome Outcome { get; set; }

    public DateTime OccurredAt { get; set; }
}
