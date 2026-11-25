
using BankingPlatform.Domain.Entities;
using BankingPlatform.Domain.ValueObjects;
using BankingPlatform.Infrastructure.Persistence;
using BankingPlatform.Infrastructure.Repositories;
using BankingPlatform.Tests.Integration.Fixtures;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BankingPlatform.Tests.Integration.Repositories;

public class CustomerRepositoryIntegrationTests
    : IClassFixture<BankingDatabaseFixture>
{
    private readonly BankingDatabaseFixture _fixture;

    public CustomerRepositoryIntegrationTests(
        BankingDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task AddAsync_AndSaveChanges_ShouldPersistCustomer()
    {
        var email = CreateUniqueEmail();
        Guid customerId = Guid.Empty;

        try
        {
            await using (var dbContext = _fixture.CreateDbContext())
            {
                var repository = new CustomerRepository(dbContext);
                var unitOfWork = new UnitOfWork(dbContext);

                var customer = Customer.Create(
                    "Integration",
                    "Test",
                    EmailAddress.Create(email));

                customerId = customer.Id;

                await repository.AddAsync(customer);
                await unitOfWork.SaveChangesAsync();
            }

            await using (var dbContext = _fixture.CreateDbContext())
            {
                var repository = new CustomerRepository(dbContext);

                var customer = await repository.GetByIdAsync(customerId);

                Assert.NotNull(customer);
                Assert.Equal("Integration", customer.FirstName);
                Assert.Equal("Test", customer.LastName);
                Assert.Equal(email, customer.Email.Value);

                var customerByEmail = await repository.GetByEmailAsync(
                    EmailAddress.Create(email));

                Assert.NotNull(customerByEmail);
                Assert.Equal(customerId, customerByEmail.Id);

                Assert.True(
                    await repository.ExistsByEmailAsync(
                        EmailAddress.Create(email)));
            }
        }
        finally
        {
            await DeleteCustomerAsync(customerId);
        }
    }

    [Fact]
    public async Task AddAsync_WithDuplicateEmail_ShouldRejectInsert()
    {
        var email = CreateUniqueEmail();
        Guid customerId = Guid.Empty;

        try
        {
            await using (var dbContext = _fixture.CreateDbContext())
            {
                var repository = new CustomerRepository(dbContext);
                var unitOfWork = new UnitOfWork(dbContext);

                var customer = Customer.Create(
                    "First",
                    "Customer",
                    EmailAddress.Create(email));

                customerId = customer.Id;

                await repository.AddAsync(customer);
                await unitOfWork.SaveChangesAsync();
            }

            await using (var dbContext = _fixture.CreateDbContext())
            {
                var repository = new CustomerRepository(dbContext);
                var unitOfWork = new UnitOfWork(dbContext);

                var duplicateCustomer = Customer.Create(
                    "Duplicate",
                    "Customer",
                    EmailAddress.Create(email));

                await repository.AddAsync(duplicateCustomer);

                var exception = await Assert.ThrowsAsync<DbUpdateException>(
                    () => unitOfWork.SaveChangesAsync());

                var postgresException =
                    Assert.IsType<PostgresException>(
                        exception.InnerException);

                Assert.Equal("23505", postgresException.SqlState);
            }
        }
        finally
        {
            await DeleteCustomerAsync(customerId);
        }
    }

    private static string CreateUniqueEmail()
    {
        return $"integration-{Guid.NewGuid():N}@example.com";
    }

    private async Task DeleteCustomerAsync(Guid customerId)
    {
        if (customerId == Guid.Empty)
        {
            return;
        }

        await using var dbContext = _fixture.CreateDbContext();

        await dbContext.Customers
            .Where(customer => customer.Id == customerId)
            .ExecuteDeleteAsync();
    }
}
