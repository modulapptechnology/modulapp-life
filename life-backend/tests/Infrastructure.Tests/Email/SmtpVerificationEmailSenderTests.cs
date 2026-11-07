using Application.Users;
using Infrastructure.Email;

namespace Infrastructure.Tests.Email;

public class SmtpVerificationEmailSenderTests
{
    private static readonly EmailVerificationOptions _verification =
        new(TimeSpan.FromHours(1), "https://life.test/verify");

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
        var sender = new SmtpVerificationEmailSender(smtp, _verification);

        var action = () => sender.SendAsync("person@life.test", "token");

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
        var sender = new SmtpVerificationEmailSender(smtp, _verification);

        var action = () => sender.SendAsync("person@life.test", "token");

        await Assert.ThrowsAsync<InvalidOperationException>(action);
    }
}
