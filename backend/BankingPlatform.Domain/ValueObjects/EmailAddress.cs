using BankingPlatform.Domain.Exceptions;

namespace BankingPlatform.Domain.ValueObjects;

public sealed record EmailAddress
{
    public string Value { get; }

    private EmailAddress(string value)
    {
        Value = value;
    }

    public static EmailAddress Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException(
                "Email address cannot be empty.");
        }

        var normalized = value.Trim().ToLowerInvariant();

        if (normalized.Length > 254 ||
            !System.Net.Mail.MailAddress.TryCreate(
                normalized, out var parsed) ||
            parsed.Address != normalized ||
            normalized.Any(char.IsWhiteSpace))
        {
            throw new DomainException(
                "Email address is invalid.");
        }

        return new EmailAddress(normalized);
    }

    public override string ToString() => Value;
}