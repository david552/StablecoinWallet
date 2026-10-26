using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StablecoinWallet.Domain.Accounts;

namespace StablecoinWallet.Infrastructure.Persistence.Configurations;

public sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("Accounts");
        builder.HasKey(account => account.Id);

        builder.Property(account => account.Id)
            .HasConversion(id => id.Value, value => new AccountId(value))
            .ValueGeneratedNever();

        builder.Property(account => account.ExternalCustomerId)
            .HasMaxLength(Account.ExternalCustomerIdMaxLength)
            .IsRequired();

        builder.Property(account => account.Asset).HasConversion<int>().IsRequired();

        builder.Property(account => account.Balance).HasPrecision(28, Account.MoneyScale).IsRequired();

        builder.Property(account => account.CreatedAt).IsRequired();

        builder.Property(account => account.Version).IsRowVersion();

        // One account per customer and asset in this single-client version.
        builder.HasIndex(account => new { account.ExternalCustomerId, account.Asset }).IsUnique();

        // This is a transient buffer, not an EF navigation or the account's full history.
        builder.Ignore(account => account.NewLedgerEntries);
    }
}
