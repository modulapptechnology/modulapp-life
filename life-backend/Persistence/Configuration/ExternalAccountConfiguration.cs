using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configuration;

public class ExternalAccountConfiguration : IEntityTypeConfiguration<ExternalAccount>
{
    public const int ProviderMaxLength = 50;

    public const int ProviderUserIdMaxLength = 256;

    public void Configure(EntityTypeBuilder<ExternalAccount> builder)
    {
        builder.ToTable("ExternalAccounts");
        builder.Property(account => account.Provider).IsRequired().HasMaxLength(ProviderMaxLength);
        builder.Property(account => account.ProviderUserId)
            .IsRequired()
            .HasMaxLength(ProviderUserIdMaxLength);
        builder.HasOne(account => account.User)
            .WithMany(user => user.ExternalAccounts)
            .HasForeignKey(account => account.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(account => new { account.Provider, account.ProviderUserId })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
        builder.HasIndex(account => account.UserId);
    }
}
