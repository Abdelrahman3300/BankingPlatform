namespace BankingPlatform.Application.DTOs;

public sealed record RegisterCustomerRequest(
    string FirstName,
    string LastName,
    string Email);