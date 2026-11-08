using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configuration;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public const int TokenHashMaxLength = 64;

    public const int DeviceIdMaxLength = 100;

    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");
        builder.Property(token => token.TokenHash).IsRequired().HasMaxLength(TokenHashMaxLength);
        builder.Property(token => token.DeviceId).IsRequired().HasMaxLength(DeviceIdMaxLength);
        builder.Property(token => token.FamilyId).IsRequired();
        builder.Property(token => token.ExpiresAt).IsRequired();
        builder.HasOne(token => token.User)
            .WithMany()
            .HasForeignKey(token => token.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(token => token.TokenHash)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
        builder.HasIndex(token => token.UserId);
        builder.HasIndex(token => token.FamilyId);
    }
}
