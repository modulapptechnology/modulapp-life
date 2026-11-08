using Application.Tests.Support;
using Application.Users;
using Domain.Users;
using NSubstitute;

namespace Application.Tests.Users;

public class EndSessionTests
{
    private static readonly DateTimeOffset _now = new(2026, 11, 7, 12, 0, 0, TimeSpan.Zero);

    private readonly IRefreshTokenRepository _refreshTokens = Substitute.For<IRefreshTokenRepository>();

    private readonly IVerificationTokenService _tokenService = Substitute.For<IVerificationTokenService>();

    private EndSession CreateSut()
    {
        return new EndSession(
            _refreshTokens,
            _tokenService,
            new PassThroughUnitOfWork(),
            new FixedTimeProvider(_now));
    }

    [Fact]
    public async Task ExecuteAsync_WithAKnownToken_RevokesEveryTokenOfItsFamily()
    {
        var familyId = Guid.NewGuid();
        var current = new RefreshToken { FamilyId = familyId, TokenHash = "hash" };
        var previous = new RefreshToken { FamilyId = familyId, TokenHash = "previous", UsedAt = _now.UtcDateTime };
        _tokenService.Hash("token").Returns("hash");
        _refreshTokens.FindByHashAsync("hash", Arg.Any<CancellationToken>()).Returns(current);
        _refreshTokens.ListFamilyAsync(familyId, Arg.Any<CancellationToken>())
            .Returns(new List<RefreshToken> { current, previous });
        var sut = CreateSut();

        await sut.ExecuteAsync("token");

        Assert.Equal(
            (_now.UtcDateTime, _now.UtcDateTime),
            (current.RevokedAt, previous.RevokedAt));
    }

    [Fact]
    public async Task ExecuteAsync_WithoutAToken_DoesNotTouchTheRepository()
    {
        var sut = CreateSut();

        await sut.ExecuteAsync(null);

        await _refreshTokens.DidNotReceiveWithAnyArgs().FindByHashAsync(default!, default);
    }
}
