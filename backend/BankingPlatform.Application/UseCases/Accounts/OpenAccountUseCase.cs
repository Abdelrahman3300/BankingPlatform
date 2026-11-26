using BankingPlatform.Application.Abstractions;
using BankingPlatform.Application.DTOs;
using BankingPlatform.Domain.Entities;
using BankingPlatform.Domain.ValueObjects;

namespace BankingPlatform.Application.UseCases.Accounts;

public sealed class OpenAccountUseCase
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IAccountNumberGenerator _accountNumberGenerator;
    private readonly IUnitOfWork _unitOfWork;

    public OpenAccountUseCase(
        ICustomerRepository customerRepository,
        IAccountRepository accountRepository,
        IAccountNumberGenerator accountNumberGenerator,
        IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _accountRepository = accountRepository;
        _accountNumberGenerator = accountNumberGenerator;
        _unitOfWork = unitOfWork;
    }

    public async Task<AccountResponse?> ExecuteAsync(
        OpenAccountRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.CustomerId == Guid.Empty)
        {
            throw new ArgumentException(
                "Customer ID cannot be empty.",
                nameof(request.CustomerId));
        }

        // Retrieve the customer.
        var customer = await _customerRepository.GetByIdAsync(
            request.CustomerId,
            cancellationToken);

        if (customer is null)
        {
            return null;
        }

        // Only active customers can open accounts.
        customer.EnsureCanOpenAccount();

        // Validate and normalize the currency code.
        var currency = Currency.Create(request.CurrencyCode);

        // Generate the account number internally.
        var accountNumber = _accountNumberGenerator.Generate();

        // Create the account through the domain entity.
        var account = Account.Create(
            customer.Id,
            accountNumber,
            currency);

        // Track the new account.
        await _accountRepository.AddAsync(
            account,
            cancellationToken);

        // Persist the change.
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AccountResponse(
            Id: account.Id,
            CustomerId: account.CustomerId,
            AccountNumber: account.AccountNumber,
            Currency: account.Currency.Code,
            Status: (int)account.Status,
            CreatedAtUtc: account.CreatedAtUtc,
            UpdatedAtUtc: account.UpdatedAtUtc);
    }
}