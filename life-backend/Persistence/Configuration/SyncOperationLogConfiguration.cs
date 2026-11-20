using Life.Domain.Sync;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Life.Persistence.Configuration;

public class SyncOperationLogConfiguration : IEntityTypeConfiguration<SyncOperationLog>
{
    public const string TableName = "SyncOperationLogs";

    public const int EntityTypeMaxLength = 64;

    public const int ErrorCodeMaxLength = 64;

    public static readonly string KindCheckConstraintName =
        EnumCheckConstraint.Name(TableName, nameof(SyncOperationLog.Kind));

    public static readonly string StatusCheckConstraintName =
        EnumCheckConstraint.Name(TableName, nameof(SyncOperationLog.Status));

    public void Configure(EntityTypeBuilder<SyncOperationLog> builder)
    {
        builder.ToTable(TableName, table =>
        {
            table.HasCheckConstraint(
                KindCheckConstraintName,
                EnumCheckConstraint.Sql<SyncOperationKind>(nameof(SyncOperationLog.Kind)));
            table.HasCheckConstraint(
                StatusCheckConstraintName,
                EnumCheckConstraint.Sql<SyncOperationStatus>(nameof(SyncOperationLog.Status)));
        });
        builder.Property(entry => entry.OperationId).IsRequired();
        builder.Property(entry => entry.EntityType).IsRequired().HasMaxLength(EntityTypeMaxLength);
        builder.Property(entry => entry.EntityId).IsRequired();
        builder.Property(entry => entry.Kind).IsRequired();
        builder.Property(entry => entry.Status).IsRequired();
        builder.Property(entry => entry.ErrorCode).HasMaxLength(ErrorCodeMaxLength);
        builder.HasOne(entry => entry.Workspace)
            .WithMany()
            .HasForeignKey(entry => entry.WorkspaceId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entry => entry.User)
            .WithMany()
            .HasForeignKey(entry => entry.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entry => new { entry.WorkspaceId, entry.UserId, entry.OperationId })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
        builder.HasIndex(entry => entry.UserId);
    }
}
