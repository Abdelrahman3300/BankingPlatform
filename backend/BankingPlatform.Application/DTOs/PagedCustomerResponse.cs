namespace BankingPlatform.Application.DTOs;

public sealed record PagedCustomerResponse(
    IReadOnlyList<CustomerResponse> Items,
    int PageNumber,
    int PageSize,
    int TotalCount,
    int TotalPages,
    bool HasPreviousPage,
    bool HasNextPage
);