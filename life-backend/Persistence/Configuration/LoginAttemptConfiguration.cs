using Life.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Life.Persistence.Configuration;

public class LoginAttemptConfiguration : IEntityTypeConfiguration<LoginAttempt>
{
    public const int IpAddressMaxLength = 45;

    public const string OutcomeCheckConstraintName = "CK_LoginAttempts_Outcome";

    public static readonly string OutcomeCheckSql =
        $"[Outcome] IN ({string.Join(", ", Enum.GetValues<LoginOutcome>().Select(outcome => (int)outcome))})";

    public void Configure(EntityTypeBuilder<LoginAttempt> builder)
    {
        builder.ToTable("LoginAttempts", table => table.HasCheckConstraint(OutcomeCheckConstraintName, OutcomeCheckSql));
        builder.Property(attempt => attempt.Email).IsRequired().HasMaxLength(UserConfiguration.EmailMaxLength);
        builder.Property(attempt => attempt.IpAddress).HasMaxLength(IpAddressMaxLength);
        builder.Property(attempt => attempt.Outcome).IsRequired();
        builder.Property(attempt => attempt.OccurredAt).IsRequired();
        builder.HasOne(attempt => attempt.User)
            .WithMany()
            .HasForeignKey(attempt => attempt.UserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(attempt => attempt.UserId);
        builder.HasIndex(attempt => new { attempt.Email, attempt.OccurredAt });
    }
}
