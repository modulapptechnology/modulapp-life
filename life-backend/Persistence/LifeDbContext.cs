using Life.Domain.Users;
using Life.Domain.Workspaces;
using Life.Persistence.Auditing;
using Life.Persistence.Configuration;
using Life.Persistence.Conversion;
using Microsoft.EntityFrameworkCore;

namespace Life.Persistence;

public class LifeDbContext(
    DbContextOptions<LifeDbContext> options,
    TimeProvider? timeProvider = null) : DbContext(options)
{
    private readonly TimeProvider _timeProvider = timeProvider ?? TimeProvider.System;

    public DbSet<User> Users => Set<User>();

    public DbSet<ExternalAccount> ExternalAccounts => Set<ExternalAccount>();

    public DbSet<EmailVerificationToken> EmailVerificationTokens => Set<EmailVerificationToken>();

    public DbSet<LoginAttempt> LoginAttempts => Set<LoginAttempt>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<Workspace> Workspaces => Set<Workspace>();

    public DbSet<WorkspaceMember> WorkspaceMembers => Set<WorkspaceMember>();

    public DbSet<WorkspaceInvitation> WorkspaceInvitations => Set<WorkspaceInvitation>();

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        EntityAuditing.Apply(ChangeTracker, _timeProvider.GetUtcNow().UtcDateTime);

        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        EntityAuditing.Apply(ChangeTracker, _timeProvider.GetUtcNow().UtcDateTime);

        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LifeDbContext).Assembly);
        modelBuilder.ApplyBaseEntityConfiguration();
        modelBuilder.ApplyIntegrityConventions();
    }
}
