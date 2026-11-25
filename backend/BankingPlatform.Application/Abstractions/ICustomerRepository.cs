
using BankingPlatform.Domain.Entities;
using BankingPlatform.Domain.ValueObjects;

namespace BankingPlatform.Application.Abstractions;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<Customer?> GetByEmailAsync(
        EmailAddress email,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByEmailAsync(
        EmailAddress email,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Customer customer,
        CancellationToken cancellationToken = default);
}
