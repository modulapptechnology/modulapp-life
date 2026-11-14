using Life.Domain.Workspaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Life.Persistence.Configuration;

public class WorkspaceInvitationConfiguration : IEntityTypeConfiguration<WorkspaceInvitation>
{
    public const string TableName = "WorkspaceInvitations";

    public const int TokenHashMaxLength = 64;

    public static readonly string RoleCheckConstraintName =
        EnumCheckConstraint.Name(TableName, nameof(WorkspaceInvitation.Role));

    public static readonly string StatusCheckConstraintName =
        EnumCheckConstraint.Name(TableName, nameof(WorkspaceInvitation.Status));

    public void Configure(EntityTypeBuilder<WorkspaceInvitation> builder)
    {
        builder.ToTable(TableName, table =>
        {
            table.HasCheckConstraint(
                RoleCheckConstraintName,
                EnumCheckConstraint.Sql<WorkspaceRole>(nameof(WorkspaceInvitation.Role)));
            table.HasCheckConstraint(
                StatusCheckConstraintName,
                EnumCheckConstraint.Sql<InvitationStatus>(nameof(WorkspaceInvitation.Status)));
        });
        builder.Property(invitation => invitation.Email)
            .IsRequired()
            .HasMaxLength(UserConfiguration.EmailMaxLength)
            .UseCollation(UserConfiguration.CaseInsensitiveCollation);
        builder.Property(invitation => invitation.Role).IsRequired();
        builder.Property(invitation => invitation.TokenHash).IsRequired().HasMaxLength(TokenHashMaxLength);
        builder.Property(invitation => invitation.Status).IsRequired();
        builder.Property(invitation => invitation.ExpiresAt).IsRequired();
        builder.HasOne(invitation => invitation.Workspace)
            .WithMany(workspace => workspace.Invitations)
            .HasForeignKey(invitation => invitation.WorkspaceId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(invitation => invitation.TokenHash)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
        builder.HasIndex(invitation => new { invitation.WorkspaceId, invitation.Email });
    }
}
