
using BankingPlatform.Application.Abstractions;
using BankingPlatform.Application.UseCases.Customers;
using BankingPlatform.Domain.Entities;
using BankingPlatform.Domain.Exceptions;
using BankingPlatform.Domain.ValueObjects;

namespace BankingPlatform.Tests.Application.UseCases.Customers;

public sealed class GetCustomerByEmailUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_WhenCustomerExists_ReturnsCustomerResponse()
    {
        // Arrange
        var customer = Customer.Create(
            "Ahmed",
            "Hassan",
            EmailAddress.Create("ahmed@example.com"));

        var repository = new FakeCustomerRepository
        {
            CustomerToReturn = customer
        };

        var useCase = new GetCustomerByEmailUseCase(repository);

        // Act
        var result = await useCase.ExecuteAsync("  AHMED@EXAMPLE.COM  ");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(customer.Id, result.Id);
        Assert.Equal("Ahmed", result.FirstName);
        Assert.Equal("Hassan", result.LastName);
        Assert.Equal("ahmed@example.com", result.Email);
        Assert.Equal((int)customer.Status, result.Status);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCustomerDoesNotExist_ReturnsNull()
    {
        // Arrange
        var repository = new FakeCustomerRepository();
        var useCase = new GetCustomerByEmailUseCase(repository);

        // Act
        var result = await useCase.ExecuteAsync("missing@example.com");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ExecuteAsync_WhenEmailIsInvalid_ThrowsDomainException()
    {
        // Arrange
        var repository = new FakeCustomerRepository();
        var useCase = new GetCustomerByEmailUseCase(repository);

        // Act
        var action = () => useCase.ExecuteAsync("invalid-email");

        // Assert
        await Assert.ThrowsAsync<DomainException>(action);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ExecuteAsync_WhenEmailIsEmptyOrWhitespace_ThrowsArgumentException(
        string email)
    {
        // Arrange
        var repository = new FakeCustomerRepository();
        var useCase = new GetCustomerByEmailUseCase(repository);

        // Act
        var action = () => useCase.ExecuteAsync(email);

        // Assert
        await Assert.ThrowsAsync<ArgumentException>(action);
    }

    [Fact]
    public async Task ExecuteAsync_PassesCancellationTokenToRepository()
    {
        // Arrange
        var customer = Customer.Create(
            "Ahmed",
            "Hassan",
            EmailAddress.Create("ahmed@example.com"));

        var repository = new FakeCustomerRepository
        {
            CustomerToReturn = customer
        };

        var useCase = new GetCustomerByEmailUseCase(repository);
        using var cancellationSource = new CancellationTokenSource();
        var cancellationToken = cancellationSource.Token;

        // Act
        await useCase.ExecuteAsync(
            "ahmed@example.com",
            cancellationToken);

        // Assert
        Assert.Equal(cancellationToken, repository.ReceivedCancellationToken);
    }

    private sealed class FakeCustomerRepository : ICustomerRepository
    {
        public Customer? CustomerToReturn { get; init; }

        public CancellationToken ReceivedCancellationToken { get; private set; }

        public Task<Customer?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Customer?>(null);
        }

        public Task<Customer?> GetByEmailAsync(
            EmailAddress email,
            CancellationToken cancellationToken = default)
        {
            ReceivedCancellationToken = cancellationToken;

            var matches = CustomerToReturn is not null
                && CustomerToReturn.Email.Value == email.Value;

            return Task.FromResult(
                matches ? CustomerToReturn : null);
        }

        public Task<bool> ExistsByEmailAsync(
            EmailAddress email,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                CustomerToReturn is not null
                && CustomerToReturn.Email.Value == email.Value);
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
}
