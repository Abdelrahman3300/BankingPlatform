using System.Security.Cryptography;
using BankingPlatform.Application.Abstractions;

namespace BankingPlatform.Infrastructure.Services;

public sealed class AccountNumberGenerator : IAccountNumberGenerator
{
    public string Generate()
    {
        // First digit: 1–9, so the number never starts with zero.
        var firstDigit = RandomNumberGenerator.GetInt32(1, 10);

        // Remaining 15 digits: 0–9.
        var remainingDigits = new char[15];

        for (var i = 0; i < remainingDigits.Length; i++)
        {
            remainingDigits[i] =
                (char)('0' + RandomNumberGenerator.GetInt32(0, 10));
        }

        return firstDigit.ToString() + new string(remainingDigits);
    }
}