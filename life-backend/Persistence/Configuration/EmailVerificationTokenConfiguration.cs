using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configuration;

public class EmailVerificationTokenConfiguration : IEntityTypeConfiguration<EmailVerificationToken>
{
    public const int TokenHashMaxLength = 64;

    public void Configure(EntityTypeBuilder<EmailVerificationToken> builder)
    {
        builder.ToTable("EmailVerificationTokens");
        builder.Property(token => token.TokenHash).IsRequired().HasMaxLength(TokenHashMaxLength);
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
    }
}
