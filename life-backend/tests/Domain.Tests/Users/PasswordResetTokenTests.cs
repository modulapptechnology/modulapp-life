using Life.Domain.Users;

namespace Life.Domain.Tests.Users;

public class PasswordResetTokenTests
{
    private static readonly DateTime _now = new(2026, 11, 20, 12, 0, 0, DateTimeKind.Utc);

    private static PasswordResetToken CreateToken()
    {
        return new PasswordResetToken { ExpiresAt = _now.AddMinutes(15) };
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

        var canBeUsed = token.CanBeUsed(_now.AddMinutes(15));

        Assert.False(canBeUsed);
    }

    [Fact]
    public void MarkUsed_WhenUsable_StoresTheInstantAndBlocksAnotherUse()
    {
        var token = CreateToken();

        token.MarkUsed(_now);

        Assert.Equal((_now, false), (token.UsedAt, token.CanBeUsed(_now)));
    }

    [Fact]
    public void MarkUsed_WhenAlreadyUsed_ThrowsInvalidOperationException()
    {
        var token = CreateToken();
        token.MarkUsed(_now);

        var action = () => token.MarkUsed(_now);

        Assert.Throws<InvalidOperationException>(action);
    }

    [Fact]
    public void Invalidate_WhenPending_BlocksItsUse()
    {
        var token = CreateToken();

        token.Invalidate(_now);

        Assert.Equal((_now, false), (token.UsedAt, token.CanBeUsed(_now)));
    }

    [Fact]
    public void Invalidate_WhenAlreadyUsed_KeepsTheOriginalInstant()
    {
        var token = CreateToken();
        token.MarkUsed(_now);

        token.Invalidate(_now.AddMinutes(1));

        Assert.Equal(_now, token.UsedAt);
    }
}
