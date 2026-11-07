using Domain.Users;

namespace Domain.Tests.Users;

public class EmailVerificationTokenTests
{
    private static readonly DateTime _now = new(2026, 11, 7, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void CanBeUsed_BeforeExpirationAndUnused_ReturnsTrue()
    {
        var token = new EmailVerificationToken { ExpiresAt = _now.AddMinutes(1) };

        var result = token.CanBeUsed(_now);

        Assert.True(result);
    }

    [Fact]
    public void CanBeUsed_AtTheExpirationInstant_ReturnsFalse()
    {
        var token = new EmailVerificationToken { ExpiresAt = _now };

        var result = token.CanBeUsed(_now);

        Assert.False(result);
    }

    [Fact]
    public void CanBeUsed_AfterExpiration_ReturnsFalse()
    {
        var token = new EmailVerificationToken { ExpiresAt = _now.AddSeconds(-1) };

        var result = token.CanBeUsed(_now);

        Assert.False(result);
    }

    [Fact]
    public void CanBeUsed_WhenAlreadyUsed_ReturnsFalse()
    {
        var token = new EmailVerificationToken { ExpiresAt = _now.AddHours(1), UsedAt = _now.AddMinutes(-1) };

        var result = token.CanBeUsed(_now);

        Assert.False(result);
    }

    [Fact]
    public void MarkUsed_WhenValid_StoresTheUsageInstant()
    {
        var token = new EmailVerificationToken { ExpiresAt = _now.AddHours(1) };

        token.MarkUsed(_now);

        Assert.Equal(_now, token.UsedAt);
    }

    [Fact]
    public void MarkUsed_WhenAlreadyUsed_ThrowsInvalidOperationException()
    {
        var token = new EmailVerificationToken { ExpiresAt = _now.AddHours(1), UsedAt = _now.AddMinutes(-1) };

        var action = () => token.MarkUsed(_now);

        Assert.Throws<InvalidOperationException>(action);
    }

    [Fact]
    public void MarkUsed_WhenExpired_ThrowsInvalidOperationException()
    {
        var token = new EmailVerificationToken { ExpiresAt = _now.AddMinutes(-1) };

        var action = () => token.MarkUsed(_now);

        Assert.Throws<InvalidOperationException>(action);
    }
}
