using BankingPlatform.Application.UseCases.Customers;
using Microsoft.Extensions.DependencyInjection;

namespace BankingPlatform.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddScoped<RegisterCustomerUseCase>();

        return services;
    }
}