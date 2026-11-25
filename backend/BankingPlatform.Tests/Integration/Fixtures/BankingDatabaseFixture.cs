
using BankingPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace BankingPlatform.Tests.Integration.Fixtures;

public sealed class BankingDatabaseFixture : IAsyncLifetime
{
    private readonly DbContextOptions<BankingDbContext> _options;

    public BankingDatabaseFixture()
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<BankingDatabaseFixture>()
            .Build();

        var connectionString = configuration.GetConnectionString(
            "BankingTestDatabase");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "BankingTestDatabase connection string is missing.");
        }

        var connectionBuilder =
            new NpgsqlConnectionStringBuilder(connectionString);

        if (!string.Equals(
                connectionBuilder.Database,
                "bankingplatform_test",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Integration tests may only use bankingplatform_test.");
        }

        _options = new DbContextOptionsBuilder<BankingDbContext>()
            .UseNpgsql(
                connectionString,
                options => options.MigrationsAssembly(
                    typeof(BankingDbContext).Assembly.FullName))
            .Options;
    }

    public BankingDbContext CreateDbContext()
    {
        return new BankingDbContext(_options);
    }

    public async Task InitializeAsync()
    {
        await using var dbContext = CreateDbContext();

        await dbContext.Database.MigrateAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}
