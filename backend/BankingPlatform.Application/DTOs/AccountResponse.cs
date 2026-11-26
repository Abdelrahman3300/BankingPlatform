namespace BankingPlatform.Application.DTOs;

public sealed record AccountResponse(
    Guid Id,
    Guid CustomerId,
    string AccountNumber,
    string Currency,
    int Status,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);