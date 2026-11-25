
using BankingPlatform.Application.Abstractions;
using BankingPlatform.Application.UseCases.Customers;
using BankingPlatform.Domain.Entities;
using BankingPlatform.Domain.ValueObjects;

namespace BankingPlatform.Tests.Application.UseCases.Customers;

public sealed class GetCustomerByIdUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_WithExistingCustomer_ShouldReturnCustomerResponse()
    {
        // Arrange
        var customer = CreateCustomer();
        var repository = new FakeCustomerRepository
        {
            CustomerToReturn = customer
        };

        var useCase = new GetCustomerByIdUseCase(repository);

        // Act
        var result = await useCase.ExecuteAsync(customer.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(customer.Id, result.Id);
        Assert.Equal("Ahmed", result.FirstName);
        Assert.Equal("Hassan", result.LastName);
        Assert.Equal("ahmed@example.com", result.Email);
        Assert.Equal((int)customer.Status, result.Status);
        Assert.Equal(customer.CreatedAtUtc, result.CreatedAtUtc);
        Assert.Equal(customer.UpdatedAtUtc, result.UpdatedAtUtc);
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistingCustomer_ShouldReturnNull()
    {
        // Arrange
        var repository = new FakeCustomerRepository();
        var useCase = new GetCustomerByIdUseCase(repository);

        // Act
        var result = await useCase.ExecuteAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyGuid_ShouldThrowArgumentException()
    {
        // Arrange
        var repository = new FakeCustomerRepository();
        var useCase = new GetCustomerByIdUseCase(repository);

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => useCase.ExecuteAsync(Guid.Empty));

        // Assert
        Assert.Equal("customerId", exception.ParamName);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldPassCancellationTokenToRepository()
    {
        // Arrange
        var customer = CreateCustomer();
        var repository = new FakeCustomerRepository
        {
            CustomerToReturn = customer
        };

        var useCase = new GetCustomerByIdUseCase(repository);
        using var cancellationTokenSource = new CancellationTokenSource();
        var token = cancellationTokenSource.Token;

        // Act
        await useCase.ExecuteAsync(customer.Id, token);

        // Assert
        Assert.Equal(token, repository.ReceivedCancellationToken);
    }

    private static Customer CreateCustomer()
    {
        var email = EmailAddress.Create("ahmed@example.com");

        return Customer.Create(
            "Ahmed",
            "Hassan",
            email);
    }

    private sealed class FakeCustomerRepository : ICustomerRepository
    {
        public Customer? CustomerToReturn { get; init; }

        public CancellationToken ReceivedCancellationToken { get; private set; }

        public Task<Customer?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            ReceivedCancellationToken = cancellationToken;

            var customer = CustomerToReturn?.Id == id
                ? CustomerToReturn
                : null;

            return Task.FromResult(customer);
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
    }
}
