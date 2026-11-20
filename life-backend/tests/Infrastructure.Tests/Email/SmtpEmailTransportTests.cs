using Life.Infrastructure.Email;

namespace Life.Infrastructure.Tests.Email;

public class SmtpEmailTransportTests
{
    [Fact]
    public async Task SendAsync_WithoutHost_ThrowsInvalidOperationException()
    {
        var smtp = new SmtpOptions(
            string.Empty,
            SmtpOptions.DefaultPort,
            true,
            string.Empty,
            string.Empty,
            "no-reply@life.test");
        var sut = new SmtpEmailTransport(smtp);

        var action = () => sut.SendAsync("person@life.test", "subject", "body");

        await Assert.ThrowsAsync<InvalidOperationException>(action);
    }

    [Fact]
    public async Task SendAsync_WithoutFromAddress_ThrowsInvalidOperationException()
    {
        var smtp = new SmtpOptions(
            "smtp.life.test",
            SmtpOptions.DefaultPort,
            true,
            string.Empty,
            string.Empty,
            string.Empty);
        var sut = new SmtpEmailTransport(smtp);

        var action = () => sut.SendAsync("person@life.test", "subject", "body");

        await Assert.ThrowsAsync<InvalidOperationException>(action);
    }
}
