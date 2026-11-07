using Domain.Common;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace API.Tests.Database;

public sealed class SampleItem : BaseEntity
{
    public string Name { get; set; } = string.Empty;
}

public sealed class SampleChild : BaseEntity
{
    public Guid SampleItemId { get; set; }

    public SampleItem? SampleItem { get; set; }
}

public sealed class SampleCascadeChild : BaseEntity
{
    public Guid SampleItemId { get; set; }

    public SampleItem? SampleItem { get; set; }
}

public sealed class SampleDbContext(DbContextOptions<LifeDbContext> options) : LifeDbContext(options)
{
    private const string _connectionString =
        "Server=localhost;Database=life_unit;User Id=none;Password=none;TrustServerCertificate=True";

    public DbSet<SampleItem> Items => Set<SampleItem>();

    public DbSet<SampleChild> Children => Set<SampleChild>();

    public DbSet<SampleCascadeChild> CascadeChildren => Set<SampleCascadeChild>();

    public static SampleDbContext Create()
    {
        var options = new DbContextOptionsBuilder<LifeDbContext>().UseSqlServer(_connectionString).Options;

        return new SampleDbContext(options);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SampleCascadeChild>()
            .HasOne(child => child.SampleItem)
            .WithMany()
            .HasForeignKey(child => child.SampleItemId)
            .OnDelete(DeleteBehavior.Cascade);
        base.OnModelCreating(modelBuilder);
    }
}
