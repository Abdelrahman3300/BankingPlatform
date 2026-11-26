namespace BankingPlatform.Application.DTOs;

public sealed record OpenAccountRequest(
    Guid CustomerId,
    string CurrencyCode);