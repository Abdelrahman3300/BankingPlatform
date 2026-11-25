using BankingPlatform.Application.Abstractions;
using BankingPlatform.Application.DTOs;

namespace BankingPlatform.Application.UseCases.Customers;

public sealed class ActivateCustomerUseCase
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ActivateCustomerUseCase(
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CustomerResponse?> ExecuteAsync(
        Guid customerId,
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

        // Domain validates whether activation is allowed.
        customer.Activate();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

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