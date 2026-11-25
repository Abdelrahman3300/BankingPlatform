
using BankingPlatform.Application.Abstractions;
using BankingPlatform.Application.DTOs;
using BankingPlatform.Domain.ValueObjects;

namespace BankingPlatform.Application.UseCases.Customers;

public sealed class GetCustomerByEmailUseCase
{
    private readonly ICustomerRepository _customerRepository;

    public GetCustomerByEmailUseCase(
        ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<CustomerResponse?> ExecuteAsync(
        string emailAddress,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(emailAddress);

        var email = EmailAddress.Create(emailAddress);

        var customer = await _customerRepository.GetByEmailAsync(
            email,
            cancellationToken);

        if (customer is null)
        {
            return null;
        }

        return new CustomerResponse(
            Id: customer.Id,
            FirstName: customer.FirstName,
            LastName: customer.LastName,
            Email: customer.Email.Value,
            Status: (int)customer.Status,
            CreatedAtUtc: customer.CreatedAtUtc,
            UpdatedAtUtc: customer.UpdatedAtUtc);
    }
}
