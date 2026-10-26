using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StablecoinWallet.Domain.Accounts;
using StablecoinWallet.Domain.Ledger;

namespace StablecoinWallet.Infrastructure.Persistence.Configurations;

public sealed class LedgerEntryConfiguration : IEntityTypeConfiguration<LedgerEntry>
{
    public void Configure(EntityTypeBuilder<LedgerEntry> builder)
    {
        builder.ToTable("LedgerEntries");

        builder.HasKey(entry => entry.Id);

        builder.Property(entry => entry.Id)
            .HasConversion(id => id.Value, value => new LedgerEntryId(value))
            .ValueGeneratedNever();

        builder.Property(entry => entry.AccountId)
            .HasConversion(id => id.Value, value => new AccountId(value))
            .IsRequired();

        builder.Property(entry => entry.Type).HasConversion<int>().IsRequired();

        builder.Property(entry => entry.Amount).HasPrecision(28, Account.MoneyScale).IsRequired();

        builder.Property(entry => entry.BalanceAfter).HasPrecision(28, Account.MoneyScale).IsRequired();

        builder.Property(entry => entry.OperationId).IsRequired();

        builder.Property(entry => entry.CreatedAt).IsRequired();

        builder.HasOne<Account>().WithMany()
            .HasForeignKey(entry => entry.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        // An operation may have a debit and a refund, but not two entries of the same type.
        builder.HasIndex(entry => new { entry.AccountId, entry.OperationId, entry.Type }).IsUnique();
        builder.HasIndex(entry => new { entry.AccountId, entry.CreatedAt });
    }
}
