using Life.Domain.Common;
using Life.Domain.Users;

namespace Life.Domain.Tests.Users;

public class UserTests
{
    [Fact]
    public void New_WithoutAssigningValues_IsPendingAndHasNoPasswordHashOrExternalAccounts()
    {
        var user = new User();

        var state = (user.Status, user.PasswordHash, user.ExternalAccounts.Count);

        Assert.Equal((UserStatus.Pending, (string?)null, 0), state);
    }

    [Fact]
    public void Type_WhenDeclared_InheritsFromTheBaseEntityAndExposesNoPlainPasswordMember()
    {
        var passwordMembers = typeof(User).GetProperties()
            .Select(property => property.Name)
            .Where(name => name.Contains("Password", StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.Equal(
            (true, nameof(User.PasswordHash)),
            (typeof(BaseEntity).IsAssignableFrom(typeof(User)), string.Join(",", passwordMembers)));
    }
}
