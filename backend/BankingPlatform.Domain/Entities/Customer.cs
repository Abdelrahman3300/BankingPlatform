using BankingPlatform.Domain.Enums;
using BankingPlatform.Domain.Exceptions;
using BankingPlatform.Domain.ValueObjects;

namespace BankingPlatform.Domain.Entities;

public class Customer
{
    public Guid Id { get; private set; }

    public string FirstName { get; private set; } = string.Empty;

    public string LastName { get; private set; } = string.Empty;

    public EmailAddress Email { get; private set; } = null!;

    public CustomerStatus Status { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? UpdatedAtUtc { get; private set; }

    private Customer()
    {
        // Required by EF Core.
    }

    private Customer(
        string firstName,
        string lastName,
        EmailAddress email)
    {
        Id = Guid.NewGuid();

        FirstName = NormalizeName(firstName, nameof(firstName));
        LastName = NormalizeName(lastName, nameof(lastName));

        Email = email ?? throw new DomainException(
            "Email address is required.");

        Status = CustomerStatus.PendingVerification;

        CreatedAtUtc = DateTime.UtcNow;
    }

    public static Customer Create(
        string firstName,
        string lastName,
        EmailAddress email)
    {
        return new Customer(firstName, lastName, email);
    }

    public void Activate()
    {
        if (Status != CustomerStatus.PendingVerification)
        {
            throw new DomainException(
                "Only customers pending verification can be activated.");
        }

        Status = CustomerStatus.Active;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Suspend()
    {
        if (Status != CustomerStatus.Active)
        {
            throw new DomainException(
                "Only active customers can be suspended.");
        }

        Status = CustomerStatus.Suspended;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Reactivate()
    {
        if (Status != CustomerStatus.Suspended)
        {
            throw new DomainException(
                "Only suspended customers can be reactivated.");
        }

        Status = CustomerStatus.Active;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Close()
    {
        if (Status == CustomerStatus.Closed)
        {
            throw new DomainException(
                "Customer is already closed.");
        }

        Status = CustomerStatus.Closed;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void EnsureCanOpenAccount()
    {
        if (Status != CustomerStatus.Active)
        {
            throw new DomainException(
                "Only active customers can open accounts.");
        }
    }

    public void UpdateName(
        string firstName,
        string lastName)
    {
        if (Status == CustomerStatus.Closed)
        {
            throw new DomainException(
                "A closed customer cannot be updated.");
        }

        FirstName = NormalizeName(firstName, nameof(firstName));
        LastName = NormalizeName(lastName, nameof(lastName));

        UpdatedAtUtc = DateTime.UtcNow;
    }

    private static string NormalizeName(
        string value,
        string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException(
                $"{parameterName} cannot be empty.");
        }

        var normalized = value.Trim();

        if (normalized.Length > 100)
        {
            throw new DomainException(
                $"{parameterName} cannot exceed 100 characters.");
        }

        return normalized;
    }
}