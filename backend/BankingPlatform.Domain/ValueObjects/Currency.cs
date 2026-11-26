
using BankingPlatform.Domain.Exceptions;

namespace BankingPlatform.Domain.ValueObjects;

public sealed class Currency : IEquatable<Currency>
{
    private static readonly HashSet<string> SupportedCodes =
    [
        "KWD",
        "USD",
        "EUR",
        "EGP"
    ];

    public string Code { get; }

    private Currency(string code)
    {
        Code = code;
    }

    public static Currency Create(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new DomainException(
                "Currency code cannot be empty.");
        }

        var normalizedCode = code.Trim().ToUpperInvariant();

        if (normalizedCode.Length != 3 ||
            !normalizedCode.All(char.IsAsciiLetter))
        {
            throw new DomainException(
                "Currency code must contain exactly three letters.");
        }

        if (!SupportedCodes.Contains(normalizedCode))
        {
            throw new DomainException(
                $"Currency '{normalizedCode}' is not supported.");
        }

        return new Currency(normalizedCode);
    }

    public bool Equals(Currency? other)
    {
        return other is not null &&
               Code == other.Code;
    }

    public override bool Equals(object? obj)
    {
        return obj is Currency other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Code.GetHashCode(StringComparison.Ordinal);
    }

    public override string ToString()
    {
        return Code;
    }

    public static bool operator ==(
        Currency? left,
        Currency? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(
        Currency? left,
        Currency? right)
    {
        return !Equals(left, right);
    }
}
