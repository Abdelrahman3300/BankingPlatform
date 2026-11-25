using BankingPlatform.Application.Abstractions;
using BankingPlatform.Application.DTOs;
using BankingPlatform.Domain.Exceptions;

namespace BankingPlatform.Application.UseCases.Customers;

public sealed class UpdateCustomerNameUseCase
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCustomerNameUseCase(
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CustomerResponse?> ExecuteAsync(
        Guid customerId,
        string firstName,
        string lastName,
        CancellationToken cancellationToken = default)
    {
        if (customerId == Guid.Empty)
        {
            throw new ArgumentException(
                "Customer ID cannot be empty.",
                nameof(customerId));
        }

        var customer = await _customerRepository.GetByIdAsync(
            customerId,
            cancellationToken);

        if (customer is null)
        {
            return null;
        }

        customer.UpdateName(firstName, lastName);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CustomerResponse(
            customer.Id,
            customer.FirstName,
            customer.LastName,
            customer.Email.Value,
            (int)customer.Status,
            customer.CreatedAtUtc,
            customer.UpdatedAtUtc);
    }
}
