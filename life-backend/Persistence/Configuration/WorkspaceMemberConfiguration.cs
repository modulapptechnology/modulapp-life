using Domain.Workspaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configuration;

public class WorkspaceMemberConfiguration : IEntityTypeConfiguration<WorkspaceMember>
{
    public const string TableName = "WorkspaceMembers";

    public const string SingleOwnerIndexName = "IX_WorkspaceMembers_WorkspaceId_Owner";

    public static readonly string RoleCheckConstraintName =
        EnumCheckConstraint.Name(TableName, nameof(WorkspaceMember.Role));

    public void Configure(EntityTypeBuilder<WorkspaceMember> builder)
    {
        builder.ToTable(TableName, table => table.HasCheckConstraint(
            RoleCheckConstraintName,
            EnumCheckConstraint.Sql<WorkspaceRole>(nameof(WorkspaceMember.Role))));
        builder.Property(member => member.Role).IsRequired();
        builder.HasOne(member => member.Workspace)
            .WithMany(workspace => workspace.Members)
            .HasForeignKey(member => member.WorkspaceId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(member => member.User)
            .WithMany()
            .HasForeignKey(member => member.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(member => new { member.WorkspaceId, member.UserId })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
        builder.HasIndex(member => member.WorkspaceId)
            .IsUnique()
            .HasFilter($"[IsDeleted] = 0 AND [Role] = {(int)WorkspaceRole.Owner}")
            .HasDatabaseName(SingleOwnerIndexName);
        builder.HasIndex(member => member.UserId);
    }
}
