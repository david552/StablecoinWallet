using StablecoinWallet.Domain.Accounts;
using StablecoinWallet.Domain.Common;

namespace StablecoinWallet.Domain.Ledger;

public sealed class LedgerEntry : Entity<LedgerEntryId>
{
    public AccountId AccountId { get; private set; } = null!;
    public LedgerEntryType Type { get; private set; }
    public decimal Amount { get; private set; }
    public decimal BalanceAfter { get; private set; }
    public Guid OperationId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private LedgerEntry() { }

    internal LedgerEntry(AccountId accountId, LedgerEntryType type,
        decimal amount, decimal balanceAfter, Guid operationId)
        : base(new LedgerEntryId(Guid.NewGuid()))
    {
        ArgumentNullException.ThrowIfNull(accountId);

        if (accountId.Value == Guid.Empty)
            throw new ArgumentException("Account ID cannot be empty.", nameof(accountId));

        if (!Enum.IsDefined(type))
            throw new ArgumentOutOfRangeException(nameof(type));

        if (operationId == Guid.Empty)
            throw new ArgumentException("Operation ID cannot be empty.", nameof(operationId));

        if (amount == 0 || decimal.Round(amount, Account.MoneyScale) != amount)
            throw new ArgumentOutOfRangeException(nameof(amount));

        if (balanceAfter < 0 || decimal.Round(balanceAfter, Account.MoneyScale) != balanceAfter)
            throw new ArgumentOutOfRangeException(nameof(balanceAfter));

        if (type == LedgerEntryType.WithdrawalDebit ? amount > 0 : amount < 0)
            throw new ArgumentException("Amount sign does not match the entry type.", nameof(amount));

        AccountId = accountId;
        Type = type;
        Amount = amount;
        BalanceAfter = balanceAfter;
        OperationId = operationId;
        CreatedAt = DateTimeOffset.UtcNow;
    }
}

public sealed record LedgerEntryId(Guid Value);
