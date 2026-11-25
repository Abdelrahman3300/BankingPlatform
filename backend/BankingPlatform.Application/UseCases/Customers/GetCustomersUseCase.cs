using BankingPlatform.Application.Abstractions;
using BankingPlatform.Application.DTOs;

namespace BankingPlatform.Application.UseCases.Customers;

public sealed class GetCustomersUseCase
{
    private const int MaximumPageSize = 100;

    private readonly ICustomerRepository _customerRepository;

    public GetCustomersUseCase(
        ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<PagedCustomerResponse> ExecuteAsync(
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        if (pageNumber < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(pageNumber),
                "Page number must be greater than zero.");
        }

        if (pageSize < 1 || pageSize > MaximumPageSize)
        {
            throw new ArgumentOutOfRangeException(
                nameof(pageSize),
                $"Page size must be between 1 and {MaximumPageSize}.");
        }

        var (customers, totalCount) =
            await _customerRepository.GetPagedAsync(
                pageNumber,
                pageSize,
                cancellationToken);

        var items = customers
            .Select(customer => new CustomerResponse(
                Id: customer.Id,
                FirstName: customer.FirstName,
                LastName: customer.LastName,
                Email: customer.Email.Value,
                Status: (int)customer.Status,
                CreatedAtUtc: customer.CreatedAtUtc,
                UpdatedAtUtc: customer.UpdatedAtUtc))
            .ToList();

        var totalPages = (int)Math.Ceiling(
            (double)totalCount / pageSize);

        return new PagedCustomerResponse(
            Items: items,
            PageNumber: pageNumber,
            PageSize: pageSize,
            TotalCount: totalCount,
            TotalPages: totalPages,
            HasPreviousPage: pageNumber > 1,
            HasNextPage: pageNumber < totalPages);
    }
}