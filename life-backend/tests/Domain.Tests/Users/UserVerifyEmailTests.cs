using Domain.Users;

namespace Domain.Tests.Users;

public class UserVerifyEmailTests
{
    private static readonly DateTime _now = new(2026, 11, 7, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void VerifyEmail_WhenPending_StoresTheInstantAndActivatesTheUser()
    {
        var user = new User { Status = UserStatus.Pending };

        user.VerifyEmail(_now);

        Assert.Equal((_now, UserStatus.Active), (user.EmailVerifiedAt, user.Status));
    }

    [Fact]
    public void VerifyEmail_WhenBlocked_StoresTheInstantAndKeepsTheUserBlocked()
    {
        var user = new User { Status = UserStatus.Blocked };

        user.VerifyEmail(_now);

        Assert.Equal((_now, UserStatus.Blocked), (user.EmailVerifiedAt, user.Status));
    }

    [Fact]
    public void New_WithoutAssigningValues_HasNoVerifiedEmail()
    {
        var user = new User();

        var verifiedAt = user.EmailVerifiedAt;

        Assert.Null(verifiedAt);
    }
}
