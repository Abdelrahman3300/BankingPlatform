
using BankingPlatform.Application.Abstractions;
using BankingPlatform.Application.DTOs;
using BankingPlatform.Application.UseCases.Customers;
using BankingPlatform.Domain.Entities;
using BankingPlatform.Domain.Exceptions;
using BankingPlatform.Domain.ValueObjects;

namespace BankingPlatform.Tests.Application.UseCases.Customers;

public class RegisterCustomerUseCaseTests
{
    private readonly FakeCustomerRepository _repository;
    private readonly FakeUnitOfWork _unitOfWork;
    private readonly RegisterCustomerUseCase _useCase;

    public RegisterCustomerUseCaseTests()
    {
        _repository = new FakeCustomerRepository();
        _unitOfWork = new FakeUnitOfWork();

        _useCase = new RegisterCustomerUseCase(
            _repository,
            _unitOfWork);
    }

    private static RegisterCustomerRequest CreateRequest(
        string email = "john@example.com")
    {
        return new RegisterCustomerRequest(
            "John",
            "Smith",
            email);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidRequest_ShouldRegisterCustomer()
    {
        // Arrange
        var request = CreateRequest();

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.CustomerId);
        Assert.Null(result.Error);

        Assert.NotNull(_repository.AddedCustomer);
        Assert.Equal(
            result.CustomerId,
            _repository.AddedCustomer.Id);

        Assert.Equal(
            "john@example.com",
            _repository.AddedCustomer.Email.Value);

        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WithExistingEmail_ShouldReturnFailure()
    {
        // Arrange
        _repository.EmailExists = true;

        var request = CreateRequest();

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Null(result.CustomerId);
        Assert.NotNull(result.Error);

        Assert.Null(_repository.AddedCustomer);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidEmail_ShouldThrowDomainException()
    {
        // Arrange
        var request = CreateRequest("invalid-email");

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(
            () => _useCase.ExecuteAsync(request));

        Assert.Null(_repository.AddedCustomer);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSavingFails_ShouldPropagateException()
    {
        // Arrange
        _unitOfWork.ExceptionToThrow =
            new InvalidOperationException("Database save failed.");

        var request = CreateRequest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _useCase.ExecuteAsync(request));

        Assert.Equal("Database save failed.", exception.Message);

        Assert.NotNull(_repository.AddedCustomer);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldPassCancellationTokenToDependencies()
    {
        // Arrange
        using var cancellationTokenSource = new CancellationTokenSource();

        var cancellationToken = cancellationTokenSource.Token;
        var request = CreateRequest();

        // Act
        var result = await _useCase.ExecuteAsync(
            request,
            cancellationToken);

        // Assert
        Assert.True(result.Succeeded);

        Assert.Equal(
            cancellationToken,
            _repository.LastCancellationToken);

        Assert.Equal(
            cancellationToken,
            _unitOfWork.LastCancellationToken);
    }

    // Test double for ICustomerRepository.
    private sealed class FakeCustomerRepository : ICustomerRepository
    {
        public bool EmailExists { get; set; }

        public Customer? AddedCustomer { get; private set; }

        public CancellationToken LastCancellationToken { get; private set; }

        public Task<Customer?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            LastCancellationToken = cancellationToken;

            return Task.FromResult<Customer?>(null);
        }

        public Task<Customer?> GetByEmailAsync(
            EmailAddress email,
            CancellationToken cancellationToken = default)
        {
            LastCancellationToken = cancellationToken;

            return Task.FromResult<Customer?>(null);
        }

        public Task<bool> ExistsByEmailAsync(
            EmailAddress email,
            CancellationToken cancellationToken = default)
        {
            LastCancellationToken = cancellationToken;

            return Task.FromResult(EmailExists);
        }

       public Task AddAsync(
            Customer customer,
            CancellationToken cancellationToken = default)
        {
            LastCancellationToken = cancellationToken;
            AddedCustomer = customer;

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
