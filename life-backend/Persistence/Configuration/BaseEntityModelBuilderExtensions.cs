using System.Reflection;
using Life.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Life.Persistence.Configuration;

public static class BaseEntityModelBuilderExtensions
{
    private static readonly MethodInfo _configureMethod = typeof(BaseEntityModelBuilderExtensions)
        .GetMethod(nameof(Configure), BindingFlags.NonPublic | BindingFlags.Static)!;

    public static ModelBuilder ApplyBaseEntityConfiguration(this ModelBuilder modelBuilder)
    {
        var rootEntityTypes = modelBuilder.Model
            .GetEntityTypes()
            .Where(entityType => entityType.BaseType is null
                && typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            .Select(entityType => entityType.ClrType)
            .ToList();

        foreach (var clrType in rootEntityTypes)
        {
            _configureMethod.MakeGenericMethod(clrType).Invoke(null, [modelBuilder]);
        }

        return modelBuilder;
    }

    private static void Configure<TEntity>(ModelBuilder modelBuilder)
        where TEntity : BaseEntity
    {
        modelBuilder.Entity<TEntity>(builder =>
        {
            builder.HasKey(entity => entity.Id);
            builder.Property(entity => entity.Id).ValueGeneratedOnAdd();
            builder.Property(entity => entity.CreatedAt).IsRequired();
            builder.Property(entity => entity.UpdatedAt).IsRequired();
            builder.Property(entity => entity.IsDeleted).IsRequired();
            builder.Property(entity => entity.RowVersion).IsRowVersion();
            builder.HasQueryFilter(entity => !entity.IsDeleted);
        });
    }
}
