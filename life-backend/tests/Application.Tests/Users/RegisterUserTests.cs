using Life.Application.Tests.Support;
using Life.Application.Users;
using Life.Domain.Users;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Life.Application.Tests.Users;

public class RegisterUserTests
{
    private static readonly DateTimeOffset _now = new(2026, 11, 7, 12, 0, 0, TimeSpan.Zero);

    private static readonly TimeSpan _lifetime = TimeSpan.FromHours(2);

    private readonly IUserRepository _users = Substitute.For<IUserRepository>();

    private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();

    private readonly IVerificationTokenService _tokens = Substitute.For<IVerificationTokenService>();

    private readonly IVerificationEmailSender _sender = Substitute.For<IVerificationEmailSender>();

    public RegisterUserTests()
    {
        _hasher.Hash(Arg.Any<string>()).Returns("hashed-password");
        _tokens.CreateCode().Returns(new VerificationToken("plain-token", "hashed-token"));
    }

    private RegisterUser CreateSut()
    {
        return new RegisterUser(
            _users,
            _hasher,
            _tokens,
            _sender,
            new PassThroughUnitOfWork(),
            new EmailVerificationOptions(_lifetime),
            new FixedTimeProvider(_now),
            NullLogger<RegisterUser>.Instance);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidRequest_StoresTheUserWithOnlyTheHashOfThePassword()
    {
        User? stored = null;
        await _users.AddAsync(
            Arg.Do<User>(user => stored = user),
            Arg.Any<EmailVerificationToken>(),
            Arg.Any<CancellationToken>());
        var sut = CreateSut();

        var response = await sut.ExecuteAsync(new RegisterUserRequest("  person@life.test ", "Abcdefgh12"));

        Assert.Equal(
            ("person@life.test", "hashed-password", UserStatus.Pending, "person@life.test"),
            (stored?.Email, stored?.PasswordHash, stored?.Status, response?.Email));
    }

    [Fact]
    public async Task ExecuteAsync_WithValidRequest_StoresOnlyTheHashOfTheTokenWithItsExpiration()
    {
        EmailVerificationToken? stored = null;
        await _users.AddAsync(
            Arg.Any<User>(),
            Arg.Do<EmailVerificationToken>(token => stored = token),
            Arg.Any<CancellationToken>());
        var sut = CreateSut();

        await sut.ExecuteAsync(new RegisterUserRequest("person@life.test", "Abcdefgh12"));

        Assert.Equal(
            ("hashed-token", _now.UtcDateTime.Add(_lifetime), null),
            (stored?.TokenHash, stored?.ExpiresAt, stored?.UsedAt));
    }

    [Fact]
    public async Task ExecuteAsync_WithValidRequest_SendsTheVerificationEmailWithThePlainToken()
    {
        var sut = CreateSut();

        await sut.ExecuteAsync(new RegisterUserRequest("person@life.test", "Abcdefgh12"));

        await _sender.Received(1).SendAsync("person@life.test", "plain-token", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithRegisteredEmail_ReturnsNullAndSendsNoEmail()
    {
        _users.AddAsync(Arg.Any<User>(), Arg.Any<EmailVerificationToken>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new DuplicateEmailException(new InvalidOperationException())));
        var sut = CreateSut();

        var response = await sut.ExecuteAsync(new RegisterUserRequest("person@life.test", "Abcdefgh12"));

        Assert.Null(response);
        await _sender.DidNotReceiveWithAnyArgs().SendAsync(default!, default!, default);
    }

    [Fact]
    public async Task ExecuteAsync_WhenTheEmailCannotBeSent_StillReturnsTheCreatedUser()
    {
        _sender.SendAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new InvalidOperationException("SMTP is down")));
        var sut = CreateSut();

        var response = await sut.ExecuteAsync(new RegisterUserRequest("person@life.test", "Abcdefgh12"));

        Assert.Equal("person@life.test", response?.Email);
    }

    [Fact]
    public async Task ExecuteAsync_WhenTheSendIsCancelled_PropagatesTheCancellation()
    {
        _sender.SendAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new OperationCanceledException()));
        var sut = CreateSut();

        var action = () => sut.ExecuteAsync(new RegisterUserRequest("person@life.test", "Abcdefgh12"));

        await Assert.ThrowsAsync<OperationCanceledException>(action);
    }
}
