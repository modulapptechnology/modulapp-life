using Domain.Workspaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configuration;

public class WorkspaceConfiguration : IEntityTypeConfiguration<Workspace>
{
    public const string TableName = "Workspaces";

    public const int NameMaxLength = 200;

    public static readonly string TypeCheckConstraintName =
        EnumCheckConstraint.Name(TableName, nameof(Workspace.Type));

    public static readonly string StatusCheckConstraintName =
        EnumCheckConstraint.Name(TableName, nameof(Workspace.Status));

    public void Configure(EntityTypeBuilder<Workspace> builder)
    {
        builder.ToTable(TableName, table =>
        {
            table.HasCheckConstraint(
                TypeCheckConstraintName,
                EnumCheckConstraint.Sql<WorkspaceType>(nameof(Workspace.Type)));
            table.HasCheckConstraint(
                StatusCheckConstraintName,
                EnumCheckConstraint.Sql<WorkspaceStatus>(nameof(Workspace.Status)));
        });
        builder.Property(workspace => workspace.Name).IsRequired().HasMaxLength(NameMaxLength);
        builder.Property(workspace => workspace.Type).IsRequired();
        builder.Property(workspace => workspace.Status).IsRequired();
        builder.HasOne(workspace => workspace.Owner)
            .WithMany()
            .HasForeignKey(workspace => workspace.OwnerId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(workspace => workspace.OwnerId);
    }
}
