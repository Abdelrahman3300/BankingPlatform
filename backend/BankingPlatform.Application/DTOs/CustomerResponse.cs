namespace BankingPlatform.Application.DTOs;

public sealed record CustomerResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    int Status,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);