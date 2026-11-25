using BankingPlatform.Application.UseCases.Customers;
using Microsoft.Extensions.DependencyInjection;

namespace BankingPlatform.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddScoped<RegisterCustomerUseCase>();
        services.AddScoped<GetCustomerByIdUseCase>();
        services.AddScoped<GetCustomerByEmailUseCase>();
        services.AddScoped<GetCustomersUseCase>();
        services.AddScoped<ActivateCustomerUseCase>();
        services.AddScoped<SuspendCustomerUseCase>();
        services.AddScoped<ReactivateCustomerUseCase>();
        
        return services;
    }
}