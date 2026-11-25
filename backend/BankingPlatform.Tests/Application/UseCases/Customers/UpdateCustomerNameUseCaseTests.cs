using BankingPlatform.Application.Abstractions;
using BankingPlatform.Application.UseCases.Customers;
using BankingPlatform.Domain.Entities;
using BankingPlatform.Domain.Enums;
using BankingPlatform.Domain.Exceptions;
using BankingPlatform.Domain.ValueObjects;

namespace BankingPlatform.Tests.Application.UseCases.Customers;

public sealed class UpdateCustomerNameUseCaseTests
{
    private readonly FakeCustomerRepository _repository;
    private readonly FakeUnitOfWork _unitOfWork;
    private readonly UpdateCustomerNameUseCase _useCase;

    public UpdateCustomerNameUseCaseTests()
    {
        _repository = new FakeCustomerRepository();
        _unitOfWork = new FakeUnitOfWork();

        _useCase = new UpdateCustomerNameUseCase(
            _repository,
            _unitOfWork);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidNames_ShouldUpdateCustomer()
    {
        var customer = CreateCustomer();
        _repository.CustomerToReturn = customer;

        var result = await _useCase.ExecuteAsync(
            customer.Id,
            "Ahmed",
            "Hassan");

        Assert.NotNull(result);
        Assert.Equal(customer.Id, result.Id);
        Assert.Equal("Ahmed", result.FirstName);
        Assert.Equal("Hassan", result.LastName);
        Assert.Equal("john@example.com", result.Email);
        Assert.NotNull(result.UpdatedAtUtc);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WithNamesContainingWhitespace_ShouldTrimNames()
    {
        var customer = CreateCustomer();
        _repository.CustomerToReturn = customer;

        var result = await _useCase.ExecuteAsync(
            customer.Id,
            "  Ahmed  ",
            "  Hassan  ");

        Assert.NotNull(result);
        Assert.Equal("Ahmed", result.FirstName);
        Assert.Equal("Hassan", result.LastName);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WithUnknownCustomer_ShouldReturnNull()
    {
        var result = await _useCase.ExecuteAsync(
            Guid.NewGuid(),
            "Ahmed",
            "Hassan");

        Assert.Null(result);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyId_ShouldThrowArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _useCase.ExecuteAsync(
                Guid.Empty,
                "Ahmed",
                "Hassan"));

        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WithClosedCustomer_ShouldThrowDomainException()
    {
        var customer = CreateCustomer();
        customer.Close();

        _repository.CustomerToReturn = customer;

        await Assert.ThrowsAsync<DomainException>(
            () => _useCase.ExecuteAsync(
                customer.Id,
                "Ahmed",
                "Hassan"));

        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Theory]
    [InlineData("", "Hassan")]
    [InlineData("Ahmed", "")]
    [InlineData("   ", "Hassan")]
    [InlineData("Ahmed", "   ")]
    public async Task ExecuteAsync_WithEmptyName_ShouldThrowDomainException(
        string firstName,
        string lastName)
    {
        var customer = CreateCustomer();
        _repository.CustomerToReturn = customer;

        await Assert.ThrowsAsync<DomainException>(
            () => _useCase.ExecuteAsync(
                customer.Id,
                firstName,
                lastName));

        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WithNameExceeding100Characters_ShouldThrowDomainException()
    {
        var customer = CreateCustomer();
        _repository.CustomerToReturn = customer;

        var longName = new string('A', 101);

        await Assert.ThrowsAsync<DomainException>(
            () => _useCase.ExecuteAsync(
                customer.Id,
                longName,
                "Hassan"));

        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    private static Customer CreateCustomer()
    {
        return Customer.Create(
            "John",
            "Smith",
            EmailAddress.Create("john@example.com"));
    }

    private sealed class FakeCustomerRepository : ICustomerRepository
    {
        public Customer? CustomerToReturn { get; set; }

        public Task<Customer?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                CustomerToReturn?.Id == id
                    ? CustomerToReturn
                    : null);
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
