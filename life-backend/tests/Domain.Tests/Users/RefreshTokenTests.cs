using Life.Domain.Users;

namespace Life.Domain.Tests.Users;

public class RefreshTokenTests
{
    private static readonly DateTime _now = new(2026, 11, 7, 12, 0, 0, DateTimeKind.Utc);

    private static RefreshToken CreateToken()
    {
        return new RefreshToken { ExpiresAt = _now.AddDays(60) };
    }

    [Fact]
    public void CanBeUsed_WhenFreshAndNotExpired_ReturnsTrue()
    {
        var token = CreateToken();

        var canBeUsed = token.CanBeUsed(_now);

        Assert.True(canBeUsed);
    }

    [Fact]
    public void CanBeUsed_WhenExpired_ReturnsFalse()
    {
        var token = CreateToken();

        var canBeUsed = token.CanBeUsed(_now.AddDays(60));

        Assert.False(canBeUsed);
    }

    [Fact]
    public void MarkUsed_WhenUsable_StoresTheInstantAndBlocksAnotherUse()
    {
        var token = CreateToken();

        token.MarkUsed(_now);

        Assert.Equal((true, false), (token.WasUsed, token.CanBeUsed(_now)));
    }

    [Fact]
    public void MarkUsed_WhenAlreadyUsed_Throws()
    {
        var token = CreateToken();
        token.MarkUsed(_now);

        var act = () => token.MarkUsed(_now);

        Assert.Throws<InvalidOperationException>(act);
    }

    [Fact]
    public void Revoke_WhenCalledTwice_KeepsTheFirstInstant()
    {
        var token = CreateToken();
        token.Revoke(_now);

        token.Revoke(_now.AddHours(1));

        Assert.Equal((DateTime?)_now, token.RevokedAt);
    }

    [Fact]
    public void CanBeUsed_WhenRevoked_ReturnsFalse()
    {
        var token = CreateToken();
        token.Revoke(_now);

        var canBeUsed = token.CanBeUsed(_now);

        Assert.False(canBeUsed);
    }
}
