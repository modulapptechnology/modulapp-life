using Life.Domain.Users;

namespace Life.Domain.Tests.Users;

public class UserLoginTests
{
    private static readonly DateTime _now = new(2026, 11, 7, 12, 0, 0, DateTimeKind.Utc);

    private static readonly TimeSpan _lockout = TimeSpan.FromMinutes(15);

    [Fact]
    public void RegisterFailedLogin_BelowTheLimit_CountsTheAttemptWithoutLocking()
    {
        var user = new User();

        user.RegisterFailedLogin(_now, 3, _lockout);

        Assert.Equal((1, (DateTime?)null), (user.FailedLoginAttempts, user.LockedUntil));
    }

    [Fact]
    public void RegisterFailedLogin_ReachingTheLimit_LocksTheUserForTheLockoutDuration()
    {
        var user = new User { FailedLoginAttempts = 2 };

        user.RegisterFailedLogin(_now, 3, _lockout);

        Assert.Equal((3, (DateTime?)_now.Add(_lockout)), (user.FailedLoginAttempts, user.LockedUntil));
    }

    [Fact]
    public void RegisterFailedLogin_AfterTheLockoutExpired_StartsTheCountAgain()
    {
        var user = new User { FailedLoginAttempts = 3, LockedUntil = _now.AddMinutes(-1) };

        user.RegisterFailedLogin(_now, 3, _lockout);

        Assert.Equal((1, (DateTime?)null), (user.FailedLoginAttempts, user.LockedUntil));
    }

    [Fact]
    public void RegisterSuccessfulLogin_AfterFailedAttempts_ResetsTheCountAndTheLock()
    {
        var user = new User { FailedLoginAttempts = 2, LockedUntil = _now.AddMinutes(5) };

        user.RegisterSuccessfulLogin();

        Assert.Equal((0, (DateTime?)null), (user.FailedLoginAttempts, user.LockedUntil));
    }

    [Fact]
    public void IsLockedAt_BeforeTheLockEnds_ReturnsTrue()
    {
        var user = new User { LockedUntil = _now.AddMinutes(1) };

        var locked = user.IsLockedAt(_now);

        Assert.True(locked);
    }

    [Fact]
    public void IsLockedAt_WhenTheLockEnded_ReturnsFalse()
    {
        var user = new User { LockedUntil = _now };

        var locked = user.IsLockedAt(_now);

        Assert.False(locked);
    }

    [Fact]
    public void IsLockedAt_WithoutLock_ReturnsFalse()
    {
        var user = new User();

        var locked = user.IsLockedAt(_now);

        Assert.False(locked);
    }
}
