using Life.Domain.Users;

namespace Life.Domain.Tests.Users;

public class UserResetPasswordTests
{
    private static readonly DateTime _now = new(2026, 11, 20, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void ResetPassword_WithANewHash_ReplacesThePasswordHash()
    {
        var user = new User { PasswordHash = "old-hash", EmailVerifiedAt = _now.AddDays(-1) };

        user.ResetPassword("new-hash", _now);

        Assert.Equal("new-hash", user.PasswordHash);
    }

    [Fact]
    public void ResetPassword_WhenTheUserWasLockedOut_ClearsTheLockout()
    {
        var user = new User
        {
            FailedLoginAttempts = 5,
            LockedUntil = _now.AddMinutes(10),
            EmailVerifiedAt = _now.AddDays(-1),
        };

        user.ResetPassword("new-hash", _now);

        Assert.Equal((0, null), (user.FailedLoginAttempts, user.LockedUntil));
    }

    [Fact]
    public void ResetPassword_WhenTheEmailWasNotVerified_VerifiesItAndActivatesTheUser()
    {
        var user = new User { Status = UserStatus.Pending };

        user.ResetPassword("new-hash", _now);

        Assert.Equal((_now, UserStatus.Active), (user.EmailVerifiedAt, user.Status));
    }

    [Fact]
    public void ResetPassword_WhenTheEmailWasAlreadyVerified_KeepsTheOriginalVerification()
    {
        var verifiedAt = _now.AddDays(-3);
        var user = new User { Status = UserStatus.Active, EmailVerifiedAt = verifiedAt };

        user.ResetPassword("new-hash", _now);

        Assert.Equal(verifiedAt, user.EmailVerifiedAt);
    }
}
