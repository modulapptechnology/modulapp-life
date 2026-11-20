using Life.Infrastructure.Email;
using Life.Infrastructure.Tests.Support;

namespace Life.Infrastructure.Tests.Email;

public class LoggingEmailTransportTests
{
    [Fact]
    public async Task SendAsync_WhenSending_LogsOnlyTheMaskedAddress()
    {
        var logger = new CapturingLogger<LoggingEmailTransport>();
        var sut = new LoggingEmailTransport(logger);

        await sut.SendAsync("person@life.test", "Your verification code", "code 123456");

        var message = Assert.Single(logger.Entries).Message;
        Assert.Equal(
            (true, false, false),
            (
                message.Contains("p***@life.test", StringComparison.Ordinal),
                message.Contains("person@", StringComparison.Ordinal),
                message.Contains("123456", StringComparison.Ordinal)));
    }
}
