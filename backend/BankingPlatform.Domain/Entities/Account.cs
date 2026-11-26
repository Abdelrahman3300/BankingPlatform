
using BankingPlatform.Domain.Enums;
using BankingPlatform.Domain.Exceptions;
using BankingPlatform.Domain.ValueObjects;

namespace BankingPlatform.Domain.Entities;

public class Account
{
    public Guid Id { get; private set; }

    public Guid CustomerId { get; private set; }

    public string AccountNumber { get; private set; } = string.Empty;

    public Currency Currency { get; private set; } = null!;

    public AccountStatus Status { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? UpdatedAtUtc { get; private set; }

    private Account()
    {
        // Required by EF Core.
    }

    private Account(
        Guid customerId,
        string accountNumber,
        Currency currency)
    {
        if (customerId == Guid.Empty)
        {
            throw new DomainException(
                "Customer ID cannot be empty.");
        }

        Id = Guid.NewGuid();
        CustomerId = customerId;
        AccountNumber = NormalizeAccountNumber(accountNumber);
        Currency = currency ?? throw new DomainException(
            "Account currency is required.");

        Status = AccountStatus.Active;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public static Account Create(
        Guid customerId,
        string accountNumber,
        Currency currency)
    {
        return new Account(
            customerId,
            accountNumber,
            currency);
    }

    public void Freeze()
    {
        if (Status != AccountStatus.Active)
        {
            throw new DomainException(
                "Only active accounts can be frozen.");
        }

        Status = AccountStatus.Frozen;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Reactivate()
    {
        if (Status != AccountStatus.Frozen)
        {
            throw new DomainException(
                "Only frozen accounts can be reactivated.");
        }

        Status = AccountStatus.Active;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Close()
    {
        if (Status == AccountStatus.Closed)
        {
            throw new DomainException(
                "Account is already closed.");
        }

        Status = AccountStatus.Closed;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    private static string NormalizeAccountNumber(
        string accountNumber)
    {
        if (string.IsNullOrWhiteSpace(accountNumber))
        {
            throw new DomainException(
                "Account number cannot be empty.");
        }

        var normalized = accountNumber.Trim();

        if (normalized.Length < 10 ||
            normalized.Length > 20 ||
            !normalized.All(char.IsAsciiDigit))
        {
            throw new DomainException(
                "Account number must contain between 10 and 20 digits.");
        }

        return normalized;
    }
}
