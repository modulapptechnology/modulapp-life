using Life.Application.Email;
using Life.Infrastructure.Email;
using Life.Infrastructure.Tests.Support;
using Microsoft.Extensions.Logging;

namespace Life.Infrastructure.Tests.Email;

public class RetryingEmailSenderTests
{
    private const string _address = "person@life.test";

    private const string _code = "987654";

    private static readonly EmailRetryOptions _retry = new(3, TimeSpan.Zero);

    private static readonly EmailMessage _message = new(
        _address,
        EmailTemplate.EmailVerification,
        new Dictionary<string, string>
        {
            [EmailValueKeys.Code] = _code,
            [EmailValueKeys.Minutes] = "15",
        },
        "en-US");

    private readonly CapturingLogger<RetryingEmailSender> _logger = new();

    private RetryingEmailSender CreateSut(FakeEmailTransport transport)
    {
        return new RetryingEmailSender(
            new EmailTemplateRenderer(),
            transport,
            _retry,
            TimeProvider.System,
            _logger);
    }

    [Fact]
    public async Task SendAsync_WhenTheTransportWorks_SendsOnceWithTheRenderedTemplate()
    {
        var transport = new FakeEmailTransport();
        var sut = CreateSut(transport);

        await sut.SendAsync(_message);

        Assert.Equal(
            (1, _address, "Your verification code", true),
            (
                transport.Attempts,
                transport.Sent.Single().To,
                transport.Sent.Single().Subject,
                transport.Sent.Single().Body.Contains(_code, StringComparison.Ordinal)));
    }

    [Fact]
    public async Task SendAsync_WhenTheFirstAttemptsFail_RetriesUntilItSucceeds()
    {
        var transport = new FakeEmailTransport(new IOException("down"), new IOException("down"));
        var sut = CreateSut(transport);

        await sut.SendAsync(_message);

        Assert.Equal((3, 1), (transport.Attempts, transport.Sent.Count));
    }

    [Fact]
    public async Task SendAsync_WhenEveryAttemptFails_ThrowsAfterTheMaximumAndLogsTheFailure()
    {
        var transport = new FakeEmailTransport(
            new IOException("down"),
            new IOException("down"),
            new IOException("down"));
        var sut = CreateSut(transport);

        var action = () => sut.SendAsync(_message);

        await Assert.ThrowsAsync<IOException>(action);
        Assert.Equal(
            (3, 2, 1),
            (
                transport.Attempts,
                _logger.Entries.Count(entry => entry.Level == LogLevel.Warning),
                _logger.Entries.Count(entry => entry.Level == LogLevel.Error)));
    }

    [Fact]
    public async Task SendAsync_WithAConfigurationError_DoesNotRetry()
    {
        var transport = new FakeEmailTransport(new InvalidOperationException("missing host"));
        var sut = CreateSut(transport);

        var action = () => sut.SendAsync(_message);

        await Assert.ThrowsAsync<InvalidOperationException>(action);
        Assert.Equal(1, transport.Attempts);
    }

    [Fact]
    public async Task SendAsync_WhenTheSendIsCancelled_DoesNotRetry()
    {
        var transport = new FakeEmailTransport(new OperationCanceledException());
        var sut = CreateSut(transport);

        var action = () => sut.SendAsync(_message);

        await Assert.ThrowsAsync<OperationCanceledException>(action);
        Assert.Equal(1, transport.Attempts);
    }

    [Fact]
    public async Task SendAsync_WhenAttemptsFail_LogsNeitherTheFullAddressNorTheCode()
    {
        var transport = new FakeEmailTransport(new IOException("down"), new IOException("down"));
        var sut = CreateSut(transport);

        await sut.SendAsync(_message);

        Assert.DoesNotContain(
            _logger.Entries,
            entry => entry.Message.Contains(_address, StringComparison.Ordinal)
                || entry.Message.Contains(_code, StringComparison.Ordinal));
    }

    [Fact]
    public async Task SendAsync_WithNullMessage_ThrowsArgumentNullException()
    {
        var sut = CreateSut(new FakeEmailTransport());

        var action = () => sut.SendAsync(null!);

        await Assert.ThrowsAsync<ArgumentNullException>(action);
    }
}
