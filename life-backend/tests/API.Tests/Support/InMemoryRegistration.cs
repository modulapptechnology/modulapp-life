using System.Collections.Concurrent;
using Application.Common;
using Application.Users;
using Domain.Users;

namespace API.Tests.Support;

public sealed class InMemoryUserRepository : IUserRepository
{
    private readonly ConcurrentDictionary<string, User> _usersByEmail = new(StringComparer.OrdinalIgnoreCase);

    private readonly ConcurrentDictionary<string, EmailVerificationToken> _tokensByHash = new();

    public IReadOnlyCollection<User> Users => _usersByEmail.Values.ToList();

    public IReadOnlyCollection<EmailVerificationToken> Tokens => _tokensByHash.Values.ToList();

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
        _tokensByHash[token.TokenHash] = token;

        return Task.CompletedTask;
    }

    public Task<EmailVerificationToken?> FindVerificationTokenAsync(
        string tokenHash,
        CancellationToken cancellationToken = default)
    {
        _tokensByHash.TryGetValue(tokenHash, out var token);

        return Task.FromResult(token);
    }
}

public sealed class CapturingEmailSender : IVerificationEmailSender
{
    private readonly ConcurrentQueue<(string Email, string Token)> _sent = new();

    public IReadOnlyCollection<(string Email, string Token)> Sent => _sent.ToList();

    public Task SendAsync(string email, string token, CancellationToken cancellationToken = default)
    {
        _sent.Enqueue((email, token));

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
