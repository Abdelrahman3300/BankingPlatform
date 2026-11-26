
using BankingPlatform.Application.Abstractions;
using BankingPlatform.Infrastructure.Persistence;
using BankingPlatform.Infrastructure.Repositories;
using BankingPlatform.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BankingPlatform.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration
            .GetConnectionString("BankingDatabase")
            ?? throw new InvalidOperationException(
                "BankingDatabase connection string is missing.");

        services.AddDbContext<BankingDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddSingleton<IAccountNumberGenerator, AccountNumberGenerator>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
