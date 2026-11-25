
using BankingPlatform.Application.Abstractions;
using BankingPlatform.Domain.Entities;
using BankingPlatform.Domain.ValueObjects;
using BankingPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BankingPlatform.Infrastructure.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly BankingDbContext _dbContext;

    public CustomerRepository(BankingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Customer?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Customers
            .SingleOrDefaultAsync(
                customer => customer.Id == id,
                cancellationToken);
    }

    public async Task<Customer?> GetByEmailAsync(
        EmailAddress email,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Customers
            .SingleOrDefaultAsync(
                customer => customer.Email == email,
                cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(
        EmailAddress email,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Customers
            .AnyAsync(
                customer => customer.Email == email,
                cancellationToken);
    }

    public async Task AddAsync(
        Customer customer,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Customers.AddAsync(
            customer,
            cancellationToken);
    }

    public async Task<(IReadOnlyList<Customer> Customers, int TotalCount)>
    GetPagedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Customers.AsNoTracking();

        var totalCount = await query.CountAsync(cancellationToken);

        var customers = await query
            .OrderByDescending(customer => customer.CreatedAtUtc)
            .ThenBy(customer => customer.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (customers, totalCount);
    }
}



