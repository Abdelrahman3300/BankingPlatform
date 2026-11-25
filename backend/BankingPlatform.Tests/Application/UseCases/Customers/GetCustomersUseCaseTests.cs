using BankingPlatform.Application.Abstractions;
using BankingPlatform.Application.UseCases.Customers;
using BankingPlatform.Domain.Entities;
using BankingPlatform.Domain.ValueObjects;

namespace BankingPlatform.Tests.Application.UseCases.Customers;

public sealed class GetCustomersUseCaseTests
{
    private readonly FakeCustomerRepository _repository;
    private readonly GetCustomersUseCase _useCase;

    public GetCustomersUseCaseTests()
    {
        _repository = new FakeCustomerRepository();
        _useCase = new GetCustomersUseCase(_repository);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidRequest_ShouldReturnPagedCustomers()
    {
        var customer = CreateCustomer();

        _repository.CustomersToReturn = new List<Customer>
        {
            customer
        };

        _repository.TotalCountToReturn = 21;

        var result = await _useCase.ExecuteAsync(
            pageNumber: 2,
            pageSize: 10);

        Assert.Single(result.Items);
        Assert.Equal(customer.Id, result.Items[0].Id);
        Assert.Equal(customer.Email.Value, result.Items[0].Email);

        Assert.Equal(2, result.PageNumber);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(21, result.TotalCount);
        Assert.Equal(3, result.TotalPages);

        Assert.True(result.HasPreviousPage);
        Assert.True(result.HasNextPage);

        Assert.Equal(2, _repository.ReceivedPageNumber);
        Assert.Equal(10, _repository.ReceivedPageSize);
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyRepository_ShouldReturnEmptyResult()
    {
        var result = await _useCase.ExecuteAsync();

        Assert.Empty(result.Items);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(0, result.TotalCount);
        Assert.Equal(0, result.TotalPages);
        Assert.False(result.HasPreviousPage);
        Assert.False(result.HasNextPage);
    }

    [Fact]
    public async Task ExecuteAsync_WithLastPage_ShouldNotHaveNextPage()
    {
        _repository.TotalCountToReturn = 21;

        var result = await _useCase.ExecuteAsync(
            pageNumber: 3,
            pageSize: 10);

        Assert.True(result.HasPreviousPage);
        Assert.False(result.HasNextPage);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ExecuteAsync_WithInvalidPageNumber_ShouldThrow(
        int pageNumber)
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => _useCase.ExecuteAsync(pageNumber: pageNumber));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    public async Task ExecuteAsync_WithInvalidPageSize_ShouldThrow(
        int pageSize)
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => _useCase.ExecuteAsync(pageSize: pageSize));
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
        public IReadOnlyList<Customer> CustomersToReturn { get; set; }
            = Array.Empty<Customer>();

        public int TotalCountToReturn { get; set; }

        public int ReceivedPageNumber { get; private set; }

        public int ReceivedPageSize { get; private set; }

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
            ReceivedPageNumber = pageNumber;
            ReceivedPageSize = pageSize;

            return Task.FromResult<
                (IReadOnlyList<Customer> Customers, int TotalCount)>(
                (CustomersToReturn, TotalCountToReturn));
        }
    }
}