using Life.Application.Email;
using Life.Application.Tests.Support;
using Life.Application.Users;
using Life.Domain.Users;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Life.Application.Tests.Users;

public class ChangePasswordTests
{
    private static readonly DateTimeOffset _now = new(2026, 11, 20, 12, 0, 0, TimeSpan.Zero);

    private readonly IUserRepository _users = Substitute.For<IUserRepository>();

    private readonly IRefreshTokenRepository _refreshTokens = Substitute.For<IRefreshTokenRepository>();

    private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();

    private readonly IEmailSender _sender = Substitute.For<IEmailSender>();

    private readonly Guid _currentSession = Guid.NewGuid();

    public ChangePasswordTests()
    {
        _hasher.Verify("Abcdefgh12", "old-hash").Returns(true);
        _hasher.Hash("Zyxwvuts98").Returns("new-hash");
        _refreshTokens.ListActiveByUserAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(new List<RefreshToken>());
    }

    private ChangePassword CreateSut()
    {
        return new ChangePassword(
            _users,
            _refreshTokens,
            _hasher,
            _sender,
            new PassThroughUnitOfWork(),
            new FixedTimeProvider(_now),
            NullLogger<ChangePassword>.Instance);
    }

    private User StoreUser(UserStatus status = UserStatus.Active, string? locale = null)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "person@life.test",
            PasswordHash = "old-hash",
            Status = status,
            Locale = locale,
        };
        _users.FindByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);

        return user;
    }

    [Fact]
    public async Task ExecuteAsync_WithTheCurrentPassword_StoresTheHashOfTheNewOne()
    {
        var user = StoreUser();
        var sut = CreateSut();

        var outcome = await sut.ExecuteAsync(
            user.Id,
            _currentSession,
            new ChangePasswordRequest("Abcdefgh12", "Zyxwvuts98"));

        Assert.Equal((ChangePasswordOutcome.Changed, "new-hash"), (outcome, user.PasswordHash));
    }

    [Fact]
    public async Task ExecuteAsync_WithTheCurrentPassword_RevokesOnlyTheOtherSessions()
    {
        var user = StoreUser();
        var current = new RefreshToken { UserId = user.Id, FamilyId = _currentSession };
        var phone = new RefreshToken { UserId = user.Id, FamilyId = Guid.NewGuid() };
        var laptop = new RefreshToken { UserId = user.Id, FamilyId = Guid.NewGuid() };
        _refreshTokens.ListActiveByUserAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(new List<RefreshToken> { current, phone, laptop });
        var sut = CreateSut();

        await sut.ExecuteAsync(user.Id, _currentSession, new ChangePasswordRequest("Abcdefgh12", "Zyxwvuts98"));

        Assert.Equal(
            (null, _now.UtcDateTime, _now.UtcDateTime),
            (current.RevokedAt, phone.RevokedAt, laptop.RevokedAt));
    }

    [Fact]
    public async Task ExecuteAsync_WithoutACurrentSession_RevokesEveryActiveSession()
    {
        var user = StoreUser();
        var phone = new RefreshToken { UserId = user.Id, FamilyId = Guid.NewGuid() };
        var laptop = new RefreshToken { UserId = user.Id, FamilyId = Guid.NewGuid() };
        _refreshTokens.ListActiveByUserAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(new List<RefreshToken> { phone, laptop });
        var sut = CreateSut();

        await sut.ExecuteAsync(user.Id, null, new ChangePasswordRequest("Abcdefgh12", "Zyxwvuts98"));

        Assert.Equal((_now.UtcDateTime, _now.UtcDateTime), (phone.RevokedAt, laptop.RevokedAt));
    }

    [Fact]
    public async Task ExecuteAsync_WithTheCurrentPassword_SendsTheNoticeInTheUserLanguage()
    {
        var user = StoreUser(locale: "en");
        EmailMessage? sent = null;
        await _sender.SendAsync(Arg.Do<EmailMessage>(message => sent = message), Arg.Any<CancellationToken>());
        var sut = CreateSut();

        await sut.ExecuteAsync(user.Id, _currentSession, new ChangePasswordRequest("Abcdefgh12", "Zyxwvuts98"));

        Assert.Equal(
            ("person@life.test", EmailTemplate.PasswordChanged, "en"),
            (sent?.To, sent?.Template, sent?.Locale));
    }

    [Fact]
    public async Task ExecuteAsync_WhenTheNoticeCannotBeSent_StillChangesThePassword()
    {
        var user = StoreUser();
        _sender.SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("smtp down"));
        var sut = CreateSut();

        var outcome = await sut.ExecuteAsync(
            user.Id,
            _currentSession,
            new ChangePasswordRequest("Abcdefgh12", "Zyxwvuts98"));

        Assert.Equal((ChangePasswordOutcome.Changed, "new-hash"), (outcome, user.PasswordHash));
    }

    [Fact]
    public async Task ExecuteAsync_WithAWrongCurrentPassword_ChangesNothingAndSendsNothing()
    {
        var user = StoreUser();
        var session = new RefreshToken { UserId = user.Id, FamilyId = Guid.NewGuid() };
        _refreshTokens.ListActiveByUserAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(new List<RefreshToken> { session });
        var sut = CreateSut();

        var outcome = await sut.ExecuteAsync(
            user.Id,
            _currentSession,
            new ChangePasswordRequest("Wrongpass12", "Zyxwvuts98"));

        Assert.Equal(
            (ChangePasswordOutcome.CurrentPasswordIncorrect, "old-hash", null, 0),
            (outcome, user.PasswordHash, session.RevokedAt, _sender.ReceivedCalls().Count()));
    }

    [Fact]
    public async Task ExecuteAsync_WithANewPasswordEqualToTheCurrent_RejectsItAndKeepsThePassword()
    {
        var user = StoreUser();
        _hasher.Verify("Zyxwvuts98", "old-hash").Returns(true);
        var sut = CreateSut();

        var outcome = await sut.ExecuteAsync(
            user.Id,
            _currentSession,
            new ChangePasswordRequest("Abcdefgh12", "Zyxwvuts98"));

        Assert.Equal(
            (ChangePasswordOutcome.NewPasswordSameAsCurrent, "old-hash"),
            (outcome, user.PasswordHash));
    }

    [Theory]
    [InlineData(UserStatus.Blocked)]
    [InlineData(UserStatus.Pending)]
    public async Task ExecuteAsync_WithAnInactiveUser_ReturnsAccountUnavailable(UserStatus status)
    {
        var user = StoreUser(status);
        var sut = CreateSut();

        var outcome = await sut.ExecuteAsync(
            user.Id,
            _currentSession,
            new ChangePasswordRequest("Abcdefgh12", "Zyxwvuts98"));

        Assert.Equal((ChangePasswordOutcome.AccountUnavailable, "old-hash"), (outcome, user.PasswordHash));
    }

    [Fact]
    public async Task ExecuteAsync_WithAnUnknownUser_ReturnsAccountUnavailable()
    {
        var sut = CreateSut();

        var outcome = await sut.ExecuteAsync(
            Guid.NewGuid(),
            _currentSession,
            new ChangePasswordRequest("Abcdefgh12", "Zyxwvuts98"));

        Assert.Equal(ChangePasswordOutcome.AccountUnavailable, outcome);
    }
}
