using Life.Domain.Users;

namespace Life.Domain.Tests.Users;

public class UserChangePasswordTests
{
    [Fact]
    public void ChangePassword_WithANewHash_ReplacesThePasswordHash()
    {
        var user = new User { PasswordHash = "old-hash" };

        user.ChangePassword("new-hash");

        Assert.Equal("new-hash", user.PasswordHash);
    }

    [Fact]
    public void ChangePassword_WhenTheUserHasFailedAttempts_KeepsTheLockoutState()
    {
        var lockedUntil = new DateTime(2026, 11, 20, 12, 10, 0, DateTimeKind.Utc);
        var user = new User { FailedLoginAttempts = 2, LockedUntil = lockedUntil };

        user.ChangePassword("new-hash");

        Assert.Equal((2, lockedUntil), (user.FailedLoginAttempts, user.LockedUntil));
    }
}
