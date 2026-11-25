using BankingPlatform.Application.Abstractions;
using BankingPlatform.Application.UseCases.Customers;
using BankingPlatform.Domain.Entities;
using BankingPlatform.Domain.Exceptions;
using BankingPlatform.Domain.ValueObjects;
using BankingPlatform.Domain.Enums;

namespace BankingPlatform.Tests.Application.UseCases.Customers;

public sealed class ReactivateCustomerUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_SuspendedCustomer_ReactivatesCustomer()
    {
        // Arrange
        var customer = CreateSuspendedCustomer();
        var repository = new FakeCustomerRepository
        {
            CustomerToReturn = customer
        };
        var unitOfWork = new FakeUnitOfWork();
        var useCase = new ReactivateCustomerUseCase(repository, unitOfWork);

        // Act
        var result = await useCase.ExecuteAsync(customer.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal((int)CustomerStatus.Active, result.Status);
        Assert.NotNull(result.UpdatedAtUtc);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_CustomerDoesNotExist_ReturnsNull()
    {
        // Arrange
        var repository = new FakeCustomerRepository();
        var unitOfWork = new FakeUnitOfWork();
        var useCase = new ReactivateCustomerUseCase(repository, unitOfWork);

        // Act
        var result = await useCase.ExecuteAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_EmptyCustomerId_ThrowsArgumentException()
    {
        // Arrange
        var repository = new FakeCustomerRepository();
        var unitOfWork = new FakeUnitOfWork();
        var useCase = new ReactivateCustomerUseCase(repository, unitOfWork);

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => useCase.ExecuteAsync(Guid.Empty));

        // Assert
        Assert.Equal("customerId", exception.ParamName);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_ActiveCustomer_ThrowsDomainException()
    {
        // Arrange
        var customer = CreateActiveCustomer();
        var repository = new FakeCustomerRepository
        {
            CustomerToReturn = customer
        };
        var unitOfWork = new FakeUnitOfWork();
        var useCase = new ReactivateCustomerUseCase(repository, unitOfWork);

        // Act
        await Assert.ThrowsAsync<DomainException>(
            () => useCase.ExecuteAsync(customer.Id));

        // Assert
        Assert.Equal((int)CustomerStatus.Active, (int)customer.Status);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    private static Customer CreateSuspendedCustomer()
    {
        var customer = Customer.Create(
            "Ahmed",
            "Hassan",
            EmailAddress.Create("reactivate.test@example.com"));

        customer.Activate();
        customer.Suspend();

        return customer;
    }

    private static Customer CreateActiveCustomer()
    {
        var customer = Customer.Create(
            "Ahmed",
            "Hassan",
            EmailAddress.Create("active.test@example.com"));

        customer.Activate();

        return customer;
    }

    private sealed class FakeCustomerRepository : ICustomerRepository
    {
        public Customer? CustomerToReturn { get; set; }

        public Task<Customer?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                CustomerToReturn?.Id == id ? CustomerToReturn : null);
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
            CustomerToReturn = customer;
            return Task.CompletedTask;
        }

        public Task<(IReadOnlyList<Customer> Customers, int TotalCount)>
            GetPagedAsync(
                int pageNumber,
                int pageSize,
                CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                ((IReadOnlyList<Customer>)Array.Empty<Customer>(), 0));
        }
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int SaveChangesCallCount { get; private set; }

        public Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            SaveChangesCallCount++;
            return Task.FromResult(1);
        }
    }
}