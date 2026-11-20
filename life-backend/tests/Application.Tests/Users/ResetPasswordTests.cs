using Life.Application.Tests.Support;
using Life.Application.Users;
using Life.Domain.Users;
using NSubstitute;

namespace Life.Application.Tests.Users;

public class ResetPasswordTests
{
    private static readonly DateTimeOffset _now = new(2026, 11, 20, 12, 0, 0, TimeSpan.Zero);

    private readonly IPasswordResetRepository _resets = Substitute.For<IPasswordResetRepository>();

    private readonly IRefreshTokenRepository _refreshTokens = Substitute.For<IRefreshTokenRepository>();

    private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();

    private readonly IVerificationTokenService _tokens = Substitute.For<IVerificationTokenService>();

    public ResetPasswordTests()
    {
        _tokens.Hash("plain-token").Returns("hashed-token");
        _hasher.Hash("Abcdefgh12").Returns("new-hash");
        _refreshTokens.ListActiveByUserAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(new List<RefreshToken>());
    }

    private ResetPassword CreateSut()
    {
        return new ResetPassword(
            _resets,
            _refreshTokens,
            _hasher,
            _tokens,
            new PassThroughUnitOfWork(),
            new FixedTimeProvider(_now));
    }

    private PasswordResetToken StoreToken(User user, Action<PasswordResetToken>? configure = null)
    {
        var token = new PasswordResetToken
        {
            UserId = user.Id,
            User = user,
            TokenHash = "hashed-token",
            ExpiresAt = _now.UtcDateTime.AddMinutes(15),
        };
        configure?.Invoke(token);
        _resets.FindByHashAsync("hashed-token", Arg.Any<CancellationToken>()).Returns(token);

        return token;
    }

    private static User NewUser(UserStatus status = UserStatus.Active)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            PasswordHash = "old-hash",
            Status = status,
            EmailVerifiedAt = new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc),
        };
    }

    [Fact]
    public async Task ExecuteAsync_WithAValidToken_StoresTheHashOfTheNewPasswordAndConsumesTheToken()
    {
        var user = NewUser();
        var token = StoreToken(user);
        var sut = CreateSut();

        var reset = await sut.ExecuteAsync(new ResetPasswordRequest("plain-token", "Abcdefgh12"));

        Assert.Equal((true, "new-hash", _now.UtcDateTime), (reset, user.PasswordHash, token.UsedAt));
    }

    [Fact]
    public async Task ExecuteAsync_WithAValidToken_RevokesEveryActiveSessionOfTheUser()
    {
        var user = NewUser();
        StoreToken(user);
        var phone = new RefreshToken { UserId = user.Id };
        var laptop = new RefreshToken { UserId = user.Id };
        _refreshTokens.ListActiveByUserAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(new List<RefreshToken> { phone, laptop });
        var sut = CreateSut();

        await sut.ExecuteAsync(new ResetPasswordRequest("plain-token", "Abcdefgh12"));

        Assert.Equal(
            (_now.UtcDateTime, _now.UtcDateTime),
            (phone.RevokedAt, laptop.RevokedAt));
    }

    [Fact]
    public async Task ExecuteAsync_WithAnUnknownToken_ReturnsFalseAndChangesNothing()
    {
        var sut = CreateSut();

        var reset = await sut.ExecuteAsync(new ResetPasswordRequest("other-token", "Abcdefgh12"));

        Assert.False(reset);
        _hasher.DidNotReceiveWithAnyArgs().Hash(default!);
    }

    [Fact]
    public async Task ExecuteAsync_WithAnExpiredToken_ReturnsFalseAndKeepsThePassword()
    {
        var user = NewUser();
        StoreToken(user, token => token.ExpiresAt = _now.UtcDateTime);
        var sut = CreateSut();

        var reset = await sut.ExecuteAsync(new ResetPasswordRequest("plain-token", "Abcdefgh12"));

        Assert.Equal((false, "old-hash"), (reset, user.PasswordHash));
    }

    [Fact]
    public async Task ExecuteAsync_WithATokenAlreadyUsed_ReturnsFalseAndKeepsThePassword()
    {
        var user = NewUser();
        StoreToken(user, token => token.UsedAt = _now.UtcDateTime.AddMinutes(-1));
        var sut = CreateSut();

        var reset = await sut.ExecuteAsync(new ResetPasswordRequest("plain-token", "Abcdefgh12"));

        Assert.Equal((false, "old-hash"), (reset, user.PasswordHash));
    }

    [Fact]
    public async Task ExecuteAsync_WithABlockedUser_ReturnsFalseAndKeepsThePassword()
    {
        var user = NewUser(UserStatus.Blocked);
        StoreToken(user);
        var sut = CreateSut();

        var reset = await sut.ExecuteAsync(new ResetPasswordRequest("plain-token", "Abcdefgh12"));

        Assert.Equal((false, "old-hash"), (reset, user.PasswordHash));
    }
}
