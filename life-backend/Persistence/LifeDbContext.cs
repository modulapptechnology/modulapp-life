using Microsoft.EntityFrameworkCore;
using Persistence.Auditing;
using Persistence.Configuration;
using Persistence.Conversion;

namespace Persistence;

public class LifeDbContext(
    DbContextOptions<LifeDbContext> options,
    TimeProvider? timeProvider = null) : DbContext(options)
{
    private readonly TimeProvider _timeProvider = timeProvider ?? TimeProvider.System;

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
        modelBuilder.ApplyBaseEntityConfiguration();
    }
}
