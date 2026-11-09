using Application.Tests.Support;
using Application.Users;
using Domain.Users;
using NSubstitute;

namespace Application.Tests.Users;

public class VerifyEmailTests
{
    private static readonly DateTimeOffset _now = new(2026, 11, 7, 12, 0, 0, TimeSpan.Zero);

    private readonly IUserRepository _users = Substitute.For<IUserRepository>();

    private readonly IVerificationTokenService _tokens = Substitute.For<IVerificationTokenService>();

    public VerifyEmailTests()
    {
        _tokens.Hash("123456").Returns("hashed-code");
        _tokens.Hash("000000").Returns("other-hash");
    }

    private VerifyEmail CreateSut()
    {
        return new VerifyEmail(_users, _tokens, new PassThroughUnitOfWork(), new FixedTimeProvider(_now));
    }

    private void StoreToken(EmailVerificationToken? token)
    {
        _users.FindLatestVerificationTokenAsync("person@life.test", Arg.Any<CancellationToken>())
            .Returns(token);
    }

    private EmailVerificationToken NewToken(User user, Action<EmailVerificationToken>? configure = null)
    {
        var token = new EmailVerificationToken
        {
            User = user,
            TokenHash = "hashed-code",
            ExpiresAt = _now.UtcDateTime.AddMinutes(15),
        };
        configure?.Invoke(token);

        return token;
    }

    [Fact]
    public async Task ExecuteAsync_WithTheRightCode_ConfirmsTheEmailAndActivatesTheUser()
    {
        var user = new User();
        StoreToken(NewToken(user));
        var sut = CreateSut();

        var verified = await sut.ExecuteAsync(new VerifyEmailRequest(" person@life.test ", "123456"));

        Assert.Equal(
            (true, _now.UtcDateTime, UserStatus.Active),
            (verified, user.EmailVerifiedAt, user.Status));
    }

    [Fact]
    public async Task ExecuteAsync_WithTheRightCode_MarksTheCodeAsUsed()
    {
        var token = NewToken(new User());
        StoreToken(token);
        var sut = CreateSut();

        await sut.ExecuteAsync(new VerifyEmailRequest("person@life.test", "123456"));

        Assert.Equal(_now.UtcDateTime, token.UsedAt);
    }

    [Fact]
    public async Task ExecuteAsync_WithAWrongCode_ReturnsFalseAndCountsTheFailedAttempt()
    {
        var user = new User();
        var token = NewToken(user);
        StoreToken(token);
        var sut = CreateSut();

        var verified = await sut.ExecuteAsync(new VerifyEmailRequest("person@life.test", "000000"));

        Assert.Equal(
            (false, 1, null, UserStatus.Pending),
            (verified, token.FailedAttempts, user.EmailVerifiedAt, user.Status));
    }

    [Fact]
    public async Task ExecuteAsync_WhenTheLastAttemptIsWrong_BlocksTheCodeEvenForTheRightValue()
    {
        var token = NewToken(
            new User(),
            candidate => candidate.FailedAttempts = EmailVerificationToken.MaxFailedAttempts - 1);
        StoreToken(token);
        var sut = CreateSut();
        await sut.ExecuteAsync(new VerifyEmailRequest("person@life.test", "000000"));

        var verified = await sut.ExecuteAsync(new VerifyEmailRequest("person@life.test", "123456"));

        Assert.Equal((false, EmailVerificationToken.MaxFailedAttempts), (verified, token.FailedAttempts));
    }

    [Fact]
    public async Task ExecuteAsync_WithUnknownEmail_ReturnsFalse()
    {
        StoreToken(null);
        var sut = CreateSut();

        var verified = await sut.ExecuteAsync(new VerifyEmailRequest("person@life.test", "123456"));

        Assert.False(verified);
    }

    [Fact]
    public async Task ExecuteAsync_WithExpiredCode_ReturnsFalseAndKeepsTheUserUnverified()
    {
        var user = new User();
        StoreToken(NewToken(user, token => token.ExpiresAt = _now.UtcDateTime.AddSeconds(-1)));
        var sut = CreateSut();

        var verified = await sut.ExecuteAsync(new VerifyEmailRequest("person@life.test", "123456"));

        Assert.Equal((false, null, UserStatus.Pending), (verified, user.EmailVerifiedAt, user.Status));
    }

    [Fact]
    public async Task ExecuteAsync_WithAlreadyUsedCode_ReturnsFalseAndKeepsTheFirstUsageInstant()
    {
        var firstUse = _now.UtcDateTime.AddMinutes(-5);
        var token = NewToken(new User(), candidate => candidate.UsedAt = firstUse);
        StoreToken(token);
        var sut = CreateSut();

        var verified = await sut.ExecuteAsync(new VerifyEmailRequest("person@life.test", "123456"));

        Assert.Equal((false, firstUse), (verified, token.UsedAt));
    }
}
