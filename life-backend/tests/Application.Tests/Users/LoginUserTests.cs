using Application.Tests.Support;
using Application.Users;
using Domain.Users;
using NSubstitute;

namespace Application.Tests.Users;

public class LoginUserTests
{
    private static readonly DateTimeOffset _now = new(2026, 11, 7, 12, 0, 0, TimeSpan.Zero);

    private static readonly LoginOptions _options = new(3, TimeSpan.FromMinutes(15));

    private readonly IUserRepository _users = Substitute.For<IUserRepository>();

    private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();

    private static readonly RefreshTokenOptions _refreshOptions = new(TimeSpan.FromDays(60));

    private readonly IAccessTokenIssuer _issuer = Substitute.For<IAccessTokenIssuer>();

    private readonly IVerificationTokenService _tokenService = Substitute.For<IVerificationTokenService>();

    private readonly IRefreshTokenRepository _refreshTokens = Substitute.For<IRefreshTokenRepository>();

    private readonly List<RefreshToken> _storedRefreshTokens = [];

    private readonly List<LoginAttempt> _attempts = [];

    public LoginUserTests()
    {
        _users.When(users => users.AddLoginAttempt(Arg.Any<LoginAttempt>()))
            .Do(call => _attempts.Add(call.Arg<LoginAttempt>()));
        _issuer.Issue(Arg.Any<User>(), Arg.Any<DateTime>())
            .Returns(new AccessToken("signed-token", _now.UtcDateTime.AddMinutes(15)));
        _tokenService.Create().Returns(new VerificationToken("plain-refresh", "hashed-refresh"));
        _refreshTokens.When(tokens => tokens.Add(Arg.Any<RefreshToken>()))
            .Do(call => _storedRefreshTokens.Add(call.Arg<RefreshToken>()));
    }

    private LoginUser CreateSut()
    {
        return new LoginUser(
            _users,
            _hasher,
            new SessionIssuer(_issuer, _tokenService, _refreshTokens, _refreshOptions),
            new PassThroughUnitOfWork(),
            _options,
            new FixedTimeProvider(_now));
    }

    private User StoreUser(UserStatus status = UserStatus.Active, bool passwordMatches = true)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "person@life.test",
            PasswordHash = "stored-hash",
            Status = status,
        };
        _users.FindByEmailAsync("person@life.test", Arg.Any<CancellationToken>()).Returns(user);
        _hasher.Verify("Abcdefgh12", "stored-hash").Returns(passwordMatches);

        return user;
    }

    [Fact]
    public async Task ExecuteAsync_WithValidCredentials_ReturnsTheSession()
    {
        StoreUser();
        var sut = CreateSut();

        var response = await sut.ExecuteAsync(new LoginRequest(" person@life.test ", "Abcdefgh12"), "10.0.0.1");

        Assert.Equal(
            new LoginResponse(
                "signed-token",
                "Bearer",
                _now.UtcDateTime.AddMinutes(15),
                "plain-refresh",
                _now.UtcDateTime.AddDays(60)),
            response);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidCredentials_AuditsTheSuccessWithTheOrigin()
    {
        var user = StoreUser();
        var sut = CreateSut();

        await sut.ExecuteAsync(new LoginRequest("person@life.test", "Abcdefgh12"), "10.0.0.1");

        var attempt = Assert.Single(_attempts);
        Assert.Equal(
            (user.Id, "person@life.test", "10.0.0.1", LoginOutcome.Succeeded, _now.UtcDateTime),
            (attempt.UserId, attempt.Email, attempt.IpAddress, attempt.Outcome, attempt.OccurredAt));
    }

    [Fact]
    public async Task ExecuteAsync_WithValidCredentials_ResetsTheFailedAttempts()
    {
        var user = StoreUser();
        user.FailedLoginAttempts = 2;
        var sut = CreateSut();

        await sut.ExecuteAsync(new LoginRequest("person@life.test", "Abcdefgh12"), null);

        Assert.Equal(0, user.FailedLoginAttempts);
    }

    [Fact]
    public async Task ExecuteAsync_WithWrongPassword_ReturnsNullAndCountsTheAttempt()
    {
        var user = StoreUser(passwordMatches: false);
        var sut = CreateSut();

        var response = await sut.ExecuteAsync(new LoginRequest("person@life.test", "Abcdefgh12"), null);

        Assert.Null(response);
        Assert.Equal(1, user.FailedLoginAttempts);
    }

    [Fact]
    public async Task ExecuteAsync_WithWrongPassword_AuditsTheFailure()
    {
        var user = StoreUser(passwordMatches: false);
        var sut = CreateSut();

        await sut.ExecuteAsync(new LoginRequest("person@life.test", "Abcdefgh12"), null);

        var attempt = Assert.Single(_attempts);
        Assert.Equal((user.Id, LoginOutcome.InvalidCredentials), (attempt.UserId, attempt.Outcome));
    }

    [Fact]
    public async Task ExecuteAsync_WhenTheFailedAttemptsReachTheLimit_LocksTheAccount()
    {
        var user = StoreUser(passwordMatches: false);
        user.FailedLoginAttempts = 2;
        var sut = CreateSut();

        await sut.ExecuteAsync(new LoginRequest("person@life.test", "Abcdefgh12"), null);

        Assert.Equal((DateTime?)_now.UtcDateTime.AddMinutes(15), user.LockedUntil);
    }

    [Fact]
    public async Task ExecuteAsync_WithUnknownEmail_ReturnsNullAndAuditsWithoutUser()
    {
        _users.FindByEmailAsync("ghost@life.test", Arg.Any<CancellationToken>()).Returns((User?)null);
        var sut = CreateSut();

        var response = await sut.ExecuteAsync(new LoginRequest("ghost@life.test", "Abcdefgh12"), null);

        var attempt = Assert.Single(_attempts);
        Assert.Null(response);
        Assert.Null(attempt.UserId);
        Assert.Equal(LoginOutcome.InvalidCredentials, attempt.Outcome);
    }

    [Fact]
    public async Task ExecuteAsync_WithUnknownEmail_StillVerifiesThePasswordToKeepTheSameCost()
    {
        _users.FindByEmailAsync("ghost@life.test", Arg.Any<CancellationToken>()).Returns((User?)null);
        var sut = CreateSut();

        await sut.ExecuteAsync(new LoginRequest("ghost@life.test", "Abcdefgh12"), null);

        _hasher.Received(1).Verify("Abcdefgh12", null);
    }

    [Fact]
    public async Task ExecuteAsync_WhenTheAccountIsLocked_ReturnsNullEvenWithTheRightPassword()
    {
        var user = StoreUser();
        user.LockedUntil = _now.UtcDateTime.AddMinutes(5);
        var sut = CreateSut();

        var response = await sut.ExecuteAsync(new LoginRequest("person@life.test", "Abcdefgh12"), null);

        var attempt = Assert.Single(_attempts);
        Assert.Null(response);
        Assert.Equal(LoginOutcome.AccountLocked, attempt.Outcome);
    }

    [Fact]
    public async Task ExecuteAsync_WhenTheAccountIsLocked_DoesNotExtendTheLock()
    {
        var user = StoreUser(passwordMatches: false);
        user.FailedLoginAttempts = 3;
        user.LockedUntil = _now.UtcDateTime.AddMinutes(5);
        var sut = CreateSut();

        await sut.ExecuteAsync(new LoginRequest("person@life.test", "Abcdefgh12"), null);

        Assert.Equal((3, (DateTime?)_now.UtcDateTime.AddMinutes(5)), (user.FailedLoginAttempts, user.LockedUntil));
    }

    [Theory]
    [InlineData(UserStatus.Pending)]
    [InlineData(UserStatus.Blocked)]
    public async Task ExecuteAsync_WhenTheUserIsNotActive_ReturnsNullAndAuditsTheReason(UserStatus status)
    {
        StoreUser(status);
        var sut = CreateSut();

        var response = await sut.ExecuteAsync(new LoginRequest("person@life.test", "Abcdefgh12"), null);

        var attempt = Assert.Single(_attempts);
        Assert.Null(response);
        Assert.Equal(LoginOutcome.AccountInactive, attempt.Outcome);
    }

    [Fact]
    public async Task ExecuteAsync_WithFailedLogin_DoesNotIssueAToken()
    {
        StoreUser(passwordMatches: false);
        var sut = CreateSut();

        await sut.ExecuteAsync(new LoginRequest("person@life.test", "Abcdefgh12"), null);

        _issuer.DidNotReceiveWithAnyArgs().Issue(default!, default);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidCredentials_StoresOnlyTheHashOfTheRefreshToken()
    {
        var user = StoreUser();
        var sut = CreateSut();

        await sut.ExecuteAsync(new LoginRequest("person@life.test", "Abcdefgh12", "phone-1"), null);

        var stored = Assert.Single(_storedRefreshTokens);
        Assert.Equal(
            (user.Id, "hashed-refresh", "phone-1", _now.UtcDateTime.AddDays(60)),
            (stored.UserId, stored.TokenHash, stored.DeviceId, stored.ExpiresAt));
    }

    [Fact]
    public async Task ExecuteAsync_WithoutDeviceId_GeneratesOneForTheSession()
    {
        StoreUser();
        var sut = CreateSut();

        await sut.ExecuteAsync(new LoginRequest("person@life.test", "Abcdefgh12"), null);

        var stored = Assert.Single(_storedRefreshTokens);
        Assert.False(string.IsNullOrWhiteSpace(stored.DeviceId));
    }

    [Fact]
    public async Task ExecuteAsync_WithFailedLogin_DoesNotStoreARefreshToken()
    {
        StoreUser(passwordMatches: false);
        var sut = CreateSut();

        await sut.ExecuteAsync(new LoginRequest("person@life.test", "Abcdefgh12"), null);

        Assert.Empty(_storedRefreshTokens);
    }
}
