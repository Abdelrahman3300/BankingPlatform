
using BankingPlatform.Domain.Entities;

namespace BankingPlatform.Application.Abstractions;

public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<Account?> GetByAccountNumberAsync(
        string accountNumber,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Account>> GetByCustomerIdAsync(
        Guid customerId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Account account,
        CancellationToken cancellationToken = default);
}
