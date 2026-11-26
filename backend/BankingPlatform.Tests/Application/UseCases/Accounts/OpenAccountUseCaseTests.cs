
using BankingPlatform.Application.Abstractions;
using BankingPlatform.Application.DTOs;
using BankingPlatform.Application.UseCases.Accounts;
using BankingPlatform.Domain.Entities;
using BankingPlatform.Domain.Exceptions;
using BankingPlatform.Domain.ValueObjects;

namespace BankingPlatform.Tests.Application.UseCases.Accounts;

public class OpenAccountUseCaseTests
{
    private readonly FakeCustomerRepository _customerRepository;
    private readonly FakeAccountRepository _accountRepository;
    private readonly FakeAccountNumberGenerator _accountNumberGenerator;
    private readonly FakeUnitOfWork _unitOfWork;
    private readonly OpenAccountUseCase _useCase;

    private static readonly Guid CustomerId =
        Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    private const string GeneratedAccountNumber = "1234567890123456";

    public OpenAccountUseCaseTests()
    {
        _customerRepository = new FakeCustomerRepository();
        _accountRepository = new FakeAccountRepository();
        _accountNumberGenerator = new FakeAccountNumberGenerator();
        _unitOfWork = new FakeUnitOfWork();

        _useCase = new OpenAccountUseCase(
            _customerRepository,
            _accountRepository,
            _accountNumberGenerator,
            _unitOfWork);
    }

    private static Customer CreateActiveCustomer()
    {
        var customer = Customer.Create(
            "John",
            "Smith",
            EmailAddress.Create("john@example.com"));

        customer.Activate();

        return customer;
    }

    private static OpenAccountRequest CreateRequest(
        Guid? customerId = null,
        string currencyCode = "KWD")
    {
        return new OpenAccountRequest(
            customerId ?? CustomerId,
            currencyCode);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidRequest_ShouldOpenAccount()
    {
        // Arrange
        var customer = CreateActiveCustomer();

        _customerRepository.CustomerToReturn = customer;

        var request = CreateRequest();

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        Assert.NotNull(result);

        Assert.Equal(customer.Id, result.CustomerId);
        Assert.Equal(GeneratedAccountNumber, result.AccountNumber);
        Assert.Equal("KWD", result.Currency);
        Assert.Equal(1, result.Status);

        Assert.NotNull(_accountRepository.AddedAccount);

        Assert.Equal(
            GeneratedAccountNumber,
            _accountRepository.AddedAccount.AccountNumber);

        Assert.Equal(1, _accountNumberGenerator.GenerateCallCount);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyCustomerId_ShouldThrowArgumentException()
    {
        // Arrange
        var request = CreateRequest(Guid.Empty);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _useCase.ExecuteAsync(request));

        Assert.Null(_accountRepository.AddedAccount);
        Assert.Equal(0, _accountNumberGenerator.GenerateCallCount);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WithNonexistentCustomer_ShouldReturnNull()
    {
        // Arrange
        var request = CreateRequest();

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        Assert.Null(result);
        Assert.Null(_accountRepository.AddedAccount);

        Assert.Equal(0, _accountNumberGenerator.GenerateCallCount);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WithInactiveCustomer_ShouldThrowDomainException()
    {
        // Arrange
        var customer = Customer.Create(
            "John",
            "Smith",
            EmailAddress.Create("john@example.com"));

        _customerRepository.CustomerToReturn = customer;

        var request = CreateRequest();

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(
            () => _useCase.ExecuteAsync(request));

        Assert.Null(_accountRepository.AddedAccount);

        Assert.Equal(0, _accountNumberGenerator.GenerateCallCount);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WithUnsupportedCurrency_ShouldThrowDomainException()
    {
        // Arrange
        _customerRepository.CustomerToReturn = CreateActiveCustomer();

        var request = CreateRequest(currencyCode: "XYZ");

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(
            () => _useCase.ExecuteAsync(request));

        Assert.Null(_accountRepository.AddedAccount);

        Assert.Equal(0, _accountNumberGenerator.GenerateCallCount);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WithNullRequest_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _useCase.ExecuteAsync(null!));

        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSavingFails_ShouldPropagateException()
    {
        // Arrange
        _customerRepository.CustomerToReturn = CreateActiveCustomer();

        _unitOfWork.ExceptionToThrow =
            new InvalidOperationException("Database save failed.");

        var request = CreateRequest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _useCase.ExecuteAsync(request));

        Assert.Equal("Database save failed.", exception.Message);

        Assert.NotNull(_accountRepository.AddedAccount);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldPassCancellationTokenToDependencies()
    {
        // Arrange
        _customerRepository.CustomerToReturn = CreateActiveCustomer();

        using var cancellationTokenSource = new CancellationTokenSource();

        var cancellationToken = cancellationTokenSource.Token;

        var request = CreateRequest();

        // Act
        var result = await _useCase.ExecuteAsync(
            request,
            cancellationToken);

        // Assert
        Assert.NotNull(result);

        Assert.Equal(
            cancellationToken,
            _customerRepository.LastCancellationToken);

        Assert.Equal(
            cancellationToken,
            _accountRepository.LastCancellationToken);

        Assert.Equal(
            cancellationToken,
            _unitOfWork.LastCancellationToken);
    }

    // Test double for ICustomerRepository.
    private sealed class FakeCustomerRepository : ICustomerRepository
    {
        public Customer? CustomerToReturn { get; set; }

        public CancellationToken LastCancellationToken { get; private set; }

        public Task<Customer?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            LastCancellationToken = cancellationToken;

            return Task.FromResult(CustomerToReturn);
        }

        public Task<Customer?> GetByEmailAsync(
            EmailAddress email,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Customer?>(null);
        }

        public Task<bool> ExistsByEmailAsync(
            EmailAddress email,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(false);
        }

        public Task AddAsync(
            Customer customer,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task<(IReadOnlyList<Customer> Customers, int TotalCount)>
            GetPagedAsync(
                int pageNumber,
                int pageSize,
                CancellationToken cancellationToken = default)
        {
            return Task.FromResult<
                (IReadOnlyList<Customer> Customers, int TotalCount)>(
                (Array.Empty<Customer>(), 0));
        }
    }

    // Test double for IAccountRepository.
    private sealed class FakeAccountRepository : IAccountRepository
    {
        public Account? AddedAccount { get; private set; }

        public CancellationToken LastCancellationToken { get; private set; }

        public Task<Account?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Account?>(null);
        }

        public Task<Account?> GetByAccountNumberAsync(
            string accountNumber,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Account?>(null);
        }

        public Task<IReadOnlyList<Account>> GetByCustomerIdAsync(
            Guid customerId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<Account>>(
                Array.Empty<Account>());
        }

        public Task AddAsync(
            Account account,
            CancellationToken cancellationToken = default)
        {
            LastCancellationToken = cancellationToken;
            AddedAccount = account;

            return Task.CompletedTask;
        }
    }

    // Test double for IAccountNumberGenerator.
    private sealed class FakeAccountNumberGenerator : IAccountNumberGenerator
    {
        public int GenerateCallCount { get; private set; }

        public string Generate()
        {
            GenerateCallCount++;

            return GeneratedAccountNumber;
        }
    }

    // Test double for IUnitOfWork.
    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int SaveChangesCallCount { get; private set; }

        public Exception? ExceptionToThrow { get; set; }

        public CancellationToken LastCancellationToken { get; private set; }

        public Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            LastCancellationToken = cancellationToken;
            SaveChangesCallCount++;

            if (ExceptionToThrow is not null)
            {
                throw ExceptionToThrow;
            }

            return Task.FromResult(1);
        }
    }
}
