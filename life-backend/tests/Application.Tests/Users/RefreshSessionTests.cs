using Application.Tests.Support;
using Application.Users;
using Domain.Users;
using NSubstitute;

namespace Application.Tests.Users;

public class RefreshSessionTests
{
    private static readonly DateTimeOffset _now = new(2026, 11, 7, 12, 0, 0, TimeSpan.Zero);

    private static readonly RefreshTokenOptions _options = new(TimeSpan.FromDays(60));

    private readonly IRefreshTokenRepository _refreshTokens = Substitute.For<IRefreshTokenRepository>();

    private readonly IVerificationTokenService _tokenService = Substitute.For<IVerificationTokenService>();

    private readonly IAccessTokenIssuer _issuer = Substitute.For<IAccessTokenIssuer>();

    private readonly List<RefreshToken> _added = [];

    private readonly Guid _familyId = Guid.NewGuid();

    public RefreshSessionTests()
    {
        _tokenService.Hash("old-token").Returns("old-hash");
        _tokenService.Create().Returns(new VerificationToken("new-token", "new-hash"));
        _issuer.Issue(Arg.Any<User>(), Arg.Any<DateTime>())
            .Returns(new AccessToken("new-access", _now.UtcDateTime.AddMinutes(15)));
        _refreshTokens.When(tokens => tokens.Add(Arg.Any<RefreshToken>()))
            .Do(call => _added.Add(call.Arg<RefreshToken>()));
    }

    private RefreshSession CreateSut()
    {
        return new RefreshSession(
            _refreshTokens,
            _tokenService,
            new SessionIssuer(_issuer, _tokenService, _refreshTokens, _options),
            new PassThroughUnitOfWork(),
            new FixedTimeProvider(_now));
    }

    private RefreshToken StoreToken(
        DateTime? expiresAt = null,
        DateTime? usedAt = null,
        DateTime? revokedAt = null,
        UserStatus status = UserStatus.Active)
    {
        var token = new RefreshToken
        {
            User = new User { Id = Guid.NewGuid(), Email = "person@life.test", Status = status },
            FamilyId = _familyId,
            DeviceId = "phone-1",
            TokenHash = "old-hash",
            ExpiresAt = expiresAt ?? _now.UtcDateTime.AddDays(10),
            UsedAt = usedAt,
            RevokedAt = revokedAt,
        };
        _refreshTokens.FindByHashAsync("old-hash", Arg.Any<CancellationToken>()).Returns(token);

        return token;
    }

    [Fact]
    public async Task ExecuteAsync_WithValidToken_ReturnsANewPair()
    {
        StoreToken();
        var sut = CreateSut();

        var response = await sut.ExecuteAsync(new RefreshSessionRequest("old-token"));

        Assert.Equal(
            new LoginResponse(
                "new-access",
                "Bearer",
                _now.UtcDateTime.AddMinutes(15),
                "new-token",
                _now.UtcDateTime.AddDays(60)),
            response);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidToken_InvalidatesThePreviousToken()
    {
        var previous = StoreToken();
        var sut = CreateSut();

        await sut.ExecuteAsync(new RefreshSessionRequest("old-token"));

        Assert.False(previous.CanBeUsed(_now.UtcDateTime));
    }

    [Fact]
    public async Task ExecuteAsync_WithValidToken_KeepsTheFamilyAndTheDeviceInTheNewToken()
    {
        StoreToken();
        var sut = CreateSut();

        await sut.ExecuteAsync(new RefreshSessionRequest("old-token"));

        var added = Assert.Single(_added);
        Assert.Equal((_familyId, "phone-1", "new-hash"), (added.FamilyId, added.DeviceId, added.TokenHash));
    }

    [Fact]
    public async Task ExecuteAsync_WithValidTokenAfterALongTimeOffline_StillReturnsANewPair()
    {
        StoreToken(expiresAt: _now.UtcDateTime.AddDays(1));
        var sut = CreateSut();

        var response = await sut.ExecuteAsync(new RefreshSessionRequest("old-token"));

        Assert.NotNull(response);
    }

    [Fact]
    public async Task ExecuteAsync_WithReusedToken_RevokesEveryTokenOfTheFamily()
    {
        var reused = StoreToken(usedAt: _now.UtcDateTime.AddDays(-1));
        var successor = new RefreshToken { FamilyId = _familyId, ExpiresAt = _now.UtcDateTime.AddDays(10) };
        _refreshTokens.ListFamilyAsync(_familyId, Arg.Any<CancellationToken>())
            .Returns(new List<RefreshToken> { reused, successor });
        var sut = CreateSut();

        await sut.ExecuteAsync(new RefreshSessionRequest("old-token"));

        Assert.Equal(
            ((DateTime?)_now.UtcDateTime, (DateTime?)_now.UtcDateTime),
            (reused.RevokedAt, successor.RevokedAt));
    }

    [Fact]
    public async Task ExecuteAsync_WithReusedToken_ReturnsNullAndIssuesNothing()
    {
        StoreToken(usedAt: _now.UtcDateTime.AddDays(-1));
        _refreshTokens.ListFamilyAsync(_familyId, Arg.Any<CancellationToken>())
            .Returns(new List<RefreshToken>());
        var sut = CreateSut();

        var response = await sut.ExecuteAsync(new RefreshSessionRequest("old-token"));

        Assert.Equal((true, 0), (response is null, _added.Count));
    }

    [Fact]
    public async Task ExecuteAsync_WithUnknownToken_ReturnsNull()
    {
        _refreshTokens.FindByHashAsync("old-hash", Arg.Any<CancellationToken>()).Returns((RefreshToken?)null);
        var sut = CreateSut();

        var response = await sut.ExecuteAsync(new RefreshSessionRequest("old-token"));

        Assert.Null(response);
    }

    [Fact]
    public async Task ExecuteAsync_WithExpiredToken_ReturnsNull()
    {
        StoreToken(expiresAt: _now.UtcDateTime.AddSeconds(-1));
        var sut = CreateSut();

        var response = await sut.ExecuteAsync(new RefreshSessionRequest("old-token"));

        Assert.Null(response);
    }

    [Fact]
    public async Task ExecuteAsync_WithRevokedToken_ReturnsNull()
    {
        StoreToken(revokedAt: _now.UtcDateTime.AddDays(-1));
        var sut = CreateSut();

        var response = await sut.ExecuteAsync(new RefreshSessionRequest("old-token"));

        Assert.Null(response);
    }

    [Theory]
    [InlineData(UserStatus.Pending)]
    [InlineData(UserStatus.Blocked)]
    public async Task ExecuteAsync_WhenTheUserIsNotActive_ReturnsNull(UserStatus status)
    {
        StoreToken(status: status);
        var sut = CreateSut();

        var response = await sut.ExecuteAsync(new RefreshSessionRequest("old-token"));

        Assert.Null(response);
    }
}
