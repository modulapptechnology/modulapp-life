using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Persistence.Configuration;

public static class IntegrityConventionsModelBuilderExtensions
{
    public static ModelBuilder ApplyIntegrityConventions(this ModelBuilder modelBuilder)
    {
        var undeclaredCascades = modelBuilder.Model
            .GetEntityTypes()
            .SelectMany(entityType => entityType.GetForeignKeys())
            .Where(foreignKey => !foreignKey.IsOwnership
                && foreignKey.DeleteBehavior == DeleteBehavior.Cascade
                && ((IConventionForeignKey)foreignKey).GetDeleteBehaviorConfigurationSource()
                    == ConfigurationSource.Convention)
            .ToList();

        foreach (var foreignKey in undeclaredCascades)
        {
            foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
        }

        return modelBuilder;
    }
}
