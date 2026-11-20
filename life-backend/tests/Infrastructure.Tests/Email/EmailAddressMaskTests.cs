using Life.Infrastructure.Email;

namespace Life.Infrastructure.Tests.Email;

public class EmailAddressMaskTests
{
    [Fact]
    public void Mask_WithAnAddress_KeepsOnlyTheFirstLetterAndTheDomain()
    {
        var address = "person@life.test";

        var masked = EmailAddressMask.Mask(address);

        Assert.Equal("p***@life.test", masked);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("no-at-sign")]
    [InlineData("@life.test")]
    public void Mask_WithoutAValidAddress_HidesEverything(string? address)
    {
        var masked = EmailAddressMask.Mask(address);

        Assert.Equal(EmailAddressMask.Hidden, masked);
    }
}
