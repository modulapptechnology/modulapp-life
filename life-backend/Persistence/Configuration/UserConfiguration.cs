using Life.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Life.Persistence.Configuration;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public const string CaseInsensitiveCollation = "SQL_Latin1_General_CP1_CI_AS";

    public const int EmailMaxLength = 320;

    public const int PasswordHashMaxLength = 512;

    public const int NameMaxLength = 200;

    public const int AvatarUrlMaxLength = 2048;

    public const int LocaleMaxLength = 35;

    public const int TimeZoneMaxLength = 64;

    public const string StatusCheckConstraintName = "CK_Users_Status";

    public static readonly string StatusCheckSql =
        $"[Status] IN ({string.Join(", ", Enum.GetValues<UserStatus>().Select(status => (int)status))})";

    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users", table => table.HasCheckConstraint(StatusCheckConstraintName, StatusCheckSql));
        builder.Property(user => user.Email)
            .IsRequired()
            .HasMaxLength(EmailMaxLength)
            .UseCollation(CaseInsensitiveCollation);
        builder.Property(user => user.PasswordHash).HasMaxLength(PasswordHashMaxLength);
        builder.Property(user => user.Name).IsRequired().HasMaxLength(NameMaxLength);
        builder.Property(user => user.Status).IsRequired();
        builder.Property(user => user.FailedLoginAttempts).IsRequired();
        builder.Property(user => user.AvatarUrl).HasMaxLength(AvatarUrlMaxLength);
        builder.Property(user => user.Locale).HasMaxLength(LocaleMaxLength);
        builder.Property(user => user.TimeZone).HasMaxLength(TimeZoneMaxLength);
        builder.HasIndex(user => user.Email)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
    }
}
