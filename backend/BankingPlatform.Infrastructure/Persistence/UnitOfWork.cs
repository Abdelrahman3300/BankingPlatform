
using BankingPlatform.Application.Abstractions;

namespace BankingPlatform.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly BankingDbContext _dbContext;

    public UnitOfWork(BankingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
