
using BankingPlatform.Application.Abstractions;
using BankingPlatform.Domain.Entities;
using BankingPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BankingPlatform.Infrastructure.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly BankingDbContext _dbContext;

    public AccountRepository(BankingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Account?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Accounts
            .SingleOrDefaultAsync(
                account => account.Id == id,
                cancellationToken);
    }

    public async Task<Account?> GetByAccountNumberAsync(
        string accountNumber,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Accounts
            .SingleOrDefaultAsync(
                account => account.AccountNumber == accountNumber,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Account>> GetByCustomerIdAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Accounts
            .AsNoTracking()
            .Where(account => account.CustomerId == customerId)
            .OrderBy(account => account.CreatedAtUtc)
            .ThenBy(account => account.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        Account account,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Accounts.AddAsync(
            account,
            cancellationToken);
    }
}
