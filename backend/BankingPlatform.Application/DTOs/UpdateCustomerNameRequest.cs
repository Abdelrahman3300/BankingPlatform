namespace BankingPlatform.Application.DTOs;

public sealed record UpdateCustomerNameRequest(
    string FirstName,
    string LastName);