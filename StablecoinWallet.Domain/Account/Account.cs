using StablecoinWallet.Domain.Assets;
using StablecoinWallet.Domain.Common;
using StablecoinWallet.Domain.Ledger;

namespace StablecoinWallet.Domain.Accounts;

public sealed class Account : AggregateRoot<AccountId>
{
    public const int MoneyScale = 6;
    public const int ExternalCustomerIdMaxLength = 200;
    private readonly List<LedgerEntry> _newLedgerEntries = [];
    public decimal Balance { get; private set; }
    public string ExternalCustomerId { get; private set; } = null!;
    public Asset Asset { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public uint Version { get; private set; }
    public IReadOnlyList<LedgerEntry> NewLedgerEntries => _newLedgerEntries.AsReadOnly();

    private Account() { }

    private Account(string externalCustomerId, Asset asset) : base(new AccountId(Guid.NewGuid()))
    {
        ExternalCustomerId = externalCustomerId;
        Asset = asset;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public static Account Create(string externalCustomerId, Asset asset)
    {
        if (string.IsNullOrWhiteSpace(externalCustomerId) ||
            externalCustomerId.Trim().Length > ExternalCustomerIdMaxLength)
            throw new ArgumentException("External customer ID must contain 1 to 200 characters.", nameof(externalCustomerId));

        if (!Enum.IsDefined(asset))
            throw new ArgumentOutOfRangeException(nameof(asset));

        return new Account(externalCustomerId.Trim(), asset);
    }

    public void CreditDeposit(decimal amount, Guid depositId) =>
        ApplyCredit(amount, LedgerEntryType.DepositCredit, depositId);

    public void RefundWithdrawal(decimal amount, Guid withdrawalId) =>
        ApplyCredit(amount, LedgerEntryType.WithdrawalRefund, withdrawalId);

    public void DebitWithdrawal(decimal amount, Guid withdrawalId)
    {
        EnsureValidAmount(amount);

        if (Balance < amount)
            throw new InvalidOperationException("Insufficient balance.");

        ApplyChange(-amount, LedgerEntryType.WithdrawalDebit, withdrawalId);
    }

    private void ApplyCredit(decimal amount, LedgerEntryType type, Guid operationId)
    {
        EnsureValidAmount(amount);
        ApplyChange(amount, type, operationId);
    }

    private void ApplyChange(decimal amount, LedgerEntryType type, Guid operationId)
    {
        var nextBalance = checked(Balance + amount);
        var entry = new LedgerEntry(Id, type, amount, nextBalance, operationId);

        Balance = nextBalance;

        _newLedgerEntries.Add(entry);
    }

    private static void EnsureValidAmount(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be positive.");

        if (decimal.Round(amount, MoneyScale) != amount)
            throw new ArgumentException("Amount cannot have more than six decimal places.", nameof(amount));
    }

    public void ClearNewLedgerEntries() => _newLedgerEntries.Clear();
}

public sealed record AccountId(Guid Value);
