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
        _tokens.Hash("plain-token").Returns("hashed-token");
    }

    private VerifyEmail CreateSut()
    {
        return new VerifyEmail(_users, _tokens, new PassThroughUnitOfWork(), new FixedTimeProvider(_now));
    }

    private void StoreToken(EmailVerificationToken? token)
    {
        _users.FindVerificationTokenAsync("hashed-token", Arg.Any<CancellationToken>()).Returns(token);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidToken_ConfirmsTheEmailAndMarksTheTokenAsUsed()
    {
        var user = new User();
        StoreToken(new EmailVerificationToken { User = user, ExpiresAt = _now.UtcDateTime.AddHours(1) });
        var sut = CreateSut();

        var verified = await sut.ExecuteAsync(new VerifyEmailRequest("plain-token"));

        Assert.Equal(
            (true, _now.UtcDateTime, UserStatus.Active),
            (verified, user.EmailVerifiedAt, user.Status));
    }

    [Fact]
    public async Task ExecuteAsync_WithValidToken_StoresTheUsageInstantInTheToken()
    {
        var token = new EmailVerificationToken { User = new User(), ExpiresAt = _now.UtcDateTime.AddHours(1) };
        StoreToken(token);
        var sut = CreateSut();

        await sut.ExecuteAsync(new VerifyEmailRequest("plain-token"));

        Assert.Equal(_now.UtcDateTime, token.UsedAt);
    }

    [Fact]
    public async Task ExecuteAsync_WithUnknownToken_ReturnsFalse()
    {
        StoreToken(null);
        var sut = CreateSut();

        var verified = await sut.ExecuteAsync(new VerifyEmailRequest("plain-token"));

        Assert.False(verified);
    }

    [Fact]
    public async Task ExecuteAsync_WithExpiredToken_ReturnsFalseAndKeepsTheUserUnverified()
    {
        var user = new User();
        StoreToken(new EmailVerificationToken { User = user, ExpiresAt = _now.UtcDateTime.AddSeconds(-1) });
        var sut = CreateSut();

        var verified = await sut.ExecuteAsync(new VerifyEmailRequest("plain-token"));

        Assert.Equal((false, null, UserStatus.Pending), (verified, user.EmailVerifiedAt, user.Status));
    }

    [Fact]
    public async Task ExecuteAsync_WithAlreadyUsedToken_ReturnsFalseAndKeepsTheFirstUsageInstant()
    {
        var firstUse = _now.UtcDateTime.AddMinutes(-5);
        var token = new EmailVerificationToken
        {
            User = new User(),
            ExpiresAt = _now.UtcDateTime.AddHours(1),
            UsedAt = firstUse,
        };
        StoreToken(token);
        var sut = CreateSut();

        var verified = await sut.ExecuteAsync(new VerifyEmailRequest("plain-token"));

        Assert.Equal((false, firstUse), (verified, token.UsedAt));
    }
}
