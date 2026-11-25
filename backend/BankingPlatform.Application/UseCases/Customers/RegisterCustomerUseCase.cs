using BankingPlatform.Application.Abstractions;
using BankingPlatform.Application.DTOs;
using BankingPlatform.Domain.Entities;
using BankingPlatform.Domain.ValueObjects;

namespace BankingPlatform.Application.UseCases.Customers;

public sealed class RegisterCustomerUseCase
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterCustomerUseCase(
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<RegisterCustomerResult> ExecuteAsync(
        RegisterCustomerRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Domain value object validates and normalizes the email.
        var email = EmailAddress.Create(request.Email);

        // Check whether the email is already registered.
        var emailExists = await _customerRepository.ExistsByEmailAsync(
            email,
            cancellationToken);

        if (emailExists)
        {
            return RegisterCustomerResult.Failure(
                "A customer with this email address already exists.");
        }

        // Domain entity enforces customer creation rules.
        var customer = Customer.Create(
            request.FirstName,
            request.LastName,
            email);

        // Track the new customer.
        await _customerRepository.AddAsync(
            customer,
            cancellationToken);

        // Persist the change.
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return RegisterCustomerResult.Success(customer.Id);
    }
}