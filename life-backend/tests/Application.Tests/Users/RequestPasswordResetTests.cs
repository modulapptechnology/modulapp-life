using Life.Application.Email;
using Life.Application.Tests.Support;
using Life.Application.Users;
using Life.Domain.Users;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Life.Application.Tests.Users;

public class RequestPasswordResetTests
{
    private static readonly DateTimeOffset _now = new(2026, 11, 20, 12, 0, 0, TimeSpan.Zero);

    private static readonly TimeSpan _lifetime = TimeSpan.FromMinutes(20);

    private readonly IUserRepository _users = Substitute.For<IUserRepository>();

    private readonly IPasswordResetRepository _resets = Substitute.For<IPasswordResetRepository>();

    private readonly IVerificationTokenService _tokens = Substitute.For<IVerificationTokenService>();

    private readonly IEmailSender _sender = Substitute.For<IEmailSender>();

    public RequestPasswordResetTests()
    {
        _tokens.Create().Returns(new VerificationToken("plain-token", "hashed-token"));
        _resets.ListPendingAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(new List<PasswordResetToken>());
    }

    private RequestPasswordReset CreateSut()
    {
        return new RequestPasswordReset(
            _users,
            _resets,
            _tokens,
            _sender,
            new PassThroughUnitOfWork(),
            new PasswordRecoveryOptions(_lifetime, "https://life.test/reset-password"),
            new FixedTimeProvider(_now),
            NullLogger<RequestPasswordReset>.Instance);
    }

    private User StoreUser(UserStatus status = UserStatus.Active, string? locale = null)
    {
        var user = new User { Id = Guid.NewGuid(), Email = "person@life.test", Status = status, Locale = locale };
        _users.FindByEmailAsync("person@life.test", Arg.Any<CancellationToken>()).Returns(user);

        return user;
    }

    [Fact]
    public async Task ExecuteAsync_WithAKnownEmail_StoresOnlyTheHashOfTheTokenWithItsExpiration()
    {
        var user = StoreUser();
        PasswordResetToken? stored = null;
        _resets.Add(Arg.Do<PasswordResetToken>(token => stored = token));
        var sut = CreateSut();

        await sut.ExecuteAsync(new RequestPasswordResetRequest(" person@life.test "));

        Assert.Equal(
            (user.Id, "hashed-token", _now.UtcDateTime.Add(_lifetime), null),
            (stored?.UserId, stored?.TokenHash, stored?.ExpiresAt, stored?.UsedAt));
    }

    [Fact]
    public async Task ExecuteAsync_WithAKnownEmail_SendsTheLinkWithThePlainTokenInTheUserLanguage()
    {
        StoreUser(locale: "en");
        EmailMessage? sent = null;
        await _sender.SendAsync(Arg.Do<EmailMessage>(message => sent = message), Arg.Any<CancellationToken>());
        var sut = CreateSut();

        await sut.ExecuteAsync(new RequestPasswordResetRequest("person@life.test"), "es");

        Assert.Equal(
            (
                "person@life.test",
                EmailTemplate.PasswordReset,
                "https://life.test/reset-password?token=plain-token",
                "20",
                "en"),
            (
                sent?.To,
                sent?.Template,
                sent?.Values[EmailValueKeys.Link],
                sent?.Values[EmailValueKeys.Minutes],
                sent?.Locale));
    }

    [Fact]
    public async Task ExecuteAsync_WithAPendingToken_InvalidatesThePreviousOne()
    {
        var user = StoreUser();
        var previous = new PasswordResetToken { ExpiresAt = _now.UtcDateTime.AddMinutes(5) };
        _resets.ListPendingAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(new List<PasswordResetToken> { previous });
        var sut = CreateSut();

        await sut.ExecuteAsync(new RequestPasswordResetRequest("person@life.test"));

        Assert.Equal(_now.UtcDateTime, previous.UsedAt);
    }

    [Fact]
    public async Task ExecuteAsync_WithAnUnknownEmail_StoresNothingAndSendsNothing()
    {
        var sut = CreateSut();

        await sut.ExecuteAsync(new RequestPasswordResetRequest("nobody@life.test"));

        _resets.DidNotReceiveWithAnyArgs().Add(default!);
        await _sender.DidNotReceiveWithAnyArgs().SendAsync(default!, default);
    }

    [Fact]
    public async Task ExecuteAsync_WithABlockedUser_StoresNothingAndSendsNothing()
    {
        StoreUser(UserStatus.Blocked);
        var sut = CreateSut();

        await sut.ExecuteAsync(new RequestPasswordResetRequest("person@life.test"));

        _resets.DidNotReceiveWithAnyArgs().Add(default!);
        await _sender.DidNotReceiveWithAnyArgs().SendAsync(default!, default);
    }

    [Fact]
    public async Task ExecuteAsync_WhenTheEmailCannotBeSent_CompletesWithoutThrowing()
    {
        StoreUser();
        _sender.SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("smtp down"));
        var sut = CreateSut();

        var exception = await Record.ExceptionAsync(
            () => sut.ExecuteAsync(new RequestPasswordResetRequest("person@life.test")));

        Assert.Null(exception);
    }
}
