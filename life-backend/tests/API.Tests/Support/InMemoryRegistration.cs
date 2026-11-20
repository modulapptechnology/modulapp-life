using System.Collections.Concurrent;
using Life.Application.Common;
using Life.Application.Email;
using Life.Application.Users;
using Life.Domain.Users;

namespace Life.API.Tests.Support;

public sealed class InMemoryUserRepository : IUserRepository
{
    private readonly ConcurrentDictionary<string, User> _usersByEmail = new(StringComparer.OrdinalIgnoreCase);

    private readonly ConcurrentQueue<EmailVerificationToken> _tokens = new();

    private readonly ConcurrentQueue<LoginAttempt> _loginAttempts = new();

    public IReadOnlyCollection<User> Users => _usersByEmail.Values.ToList();

    public IReadOnlyCollection<EmailVerificationToken> Tokens => _tokens.ToList();

    public Task AddAsync(
        User user,
        EmailVerificationToken token,
        CancellationToken cancellationToken = default)
    {
        if (!_usersByEmail.TryAdd(user.Email, user))
        {
            throw new DuplicateEmailException(new InvalidOperationException("Duplicate key."));
        }

        user.Id = Guid.NewGuid();
        _tokens.Enqueue(token);

        return Task.CompletedTask;
    }

    public Task<EmailVerificationToken?> FindLatestVerificationTokenAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var token = _tokens.LastOrDefault(candidate =>
            string.Equals(candidate.User?.Email, email, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(token);
    }

    public Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        _usersByEmail.TryGetValue(email, out var user);

        return Task.FromResult(user);
    }

    public void AddLoginAttempt(LoginAttempt attempt)
    {
        _loginAttempts.Enqueue(attempt);
    }

    public IReadOnlyCollection<LoginAttempt> LoginAttempts => _loginAttempts.ToList();

    public void Seed(User user)
    {
        user.Id = Guid.NewGuid();
        _usersByEmail[user.Email] = user;
    }
}

public sealed class InMemoryRefreshTokenRepository(InMemoryUserRepository users) : IRefreshTokenRepository
{
    private readonly ConcurrentQueue<RefreshToken> _tokens = new();

    public IReadOnlyCollection<RefreshToken> Tokens => _tokens.ToList();

    public void Add(RefreshToken token)
    {
        token.Id = Guid.NewGuid();
        _tokens.Enqueue(token);
    }

    public Task<RefreshToken?> FindByHashAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        var token = _tokens.FirstOrDefault(candidate => candidate.TokenHash == tokenHash);

        if (token is not null)
        {
            token.User = users.Users.FirstOrDefault(user => user.Id == token.UserId);
        }

        return Task.FromResult(token);
    }

    public Task<IReadOnlyList<RefreshToken>> ListFamilyAsync(
        Guid familyId,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<RefreshToken> family = _tokens.Where(token => token.FamilyId == familyId).ToList();

        return Task.FromResult(family);
    }

    public Task<IReadOnlyList<RefreshToken>> ListActiveByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<RefreshToken> active = _tokens
            .Where(token => token.UserId == userId && token.RevokedAt is null)
            .ToList();

        return Task.FromResult(active);
    }
}

public sealed class InMemoryPasswordResetRepository(InMemoryUserRepository users) : IPasswordResetRepository
{
    private readonly ConcurrentQueue<PasswordResetToken> _tokens = new();

    public IReadOnlyCollection<PasswordResetToken> Tokens => _tokens.ToList();

    public void Add(PasswordResetToken token)
    {
        token.Id = Guid.NewGuid();
        _tokens.Enqueue(token);
    }

    public Task<PasswordResetToken?> FindByHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default)
    {
        var token = _tokens.FirstOrDefault(candidate => candidate.TokenHash == tokenHash);

        if (token is not null)
        {
            token.User = users.Users.FirstOrDefault(user => user.Id == token.UserId);
        }

        return Task.FromResult(token);
    }

    public Task<IReadOnlyList<PasswordResetToken>> ListPendingAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<PasswordResetToken> pending = _tokens
            .Where(token => token.UserId == userId && token.UsedAt is null)
            .ToList();

        return Task.FromResult(pending);
    }
}

public sealed class CapturingEmailSender : IEmailSender
{
    private readonly ConcurrentQueue<(string Email, string Token)> _sent = new();

    public IReadOnlyCollection<(string Email, string Token)> Sent => _sent.ToList();

    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        var token = message.Values.TryGetValue(EmailValueKeys.Code, out var code)
            ? code
            : message.Values[EmailValueKeys.Link];

        _sent.Enqueue((message.To, token));

        return Task.CompletedTask;
    }
}

public sealed class PassThroughUnitOfWork : IUnitOfWork
{
    public Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken = default)
    {
        return operation(cancellationToken);
    }

    public Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default)
    {
        return operation(cancellationToken);
    }
}
