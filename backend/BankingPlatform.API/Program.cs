
using BankingPlatform.Application;
using BankingPlatform.Application.DTOs;
using BankingPlatform.Application.UseCases.Customers;
using BankingPlatform.Domain.Exceptions;
using BankingPlatform.Infrastructure;
using BankingPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

// Application and infrastructure
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Build application
var app = builder.Build();

// OpenAPI
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Health endpoint
app.MapHealthChecks("/health");

// Root endpoint
app.MapGet("/", () =>
{
    return Results.Ok(new
    {
        application = "BankingPlatform",
        status = "Running",
        version = "1.0.0"
    });
});

// Status endpoint
app.MapGet("/api/v1/status", () =>
{
    return Results.Ok(new
    {
        status = "OK",
        message = "BankingPlatform API is running"
    });
});

// Database connectivity endpoint
app.MapGet("/api/v1/database/check",
    async (BankingDbContext dbContext) =>
    {
        var canConnect =
            await dbContext.Database.CanConnectAsync();

        return Results.Ok(new
        {
            database = "PostgreSQL",
            connected = canConnect
        });
    });

// Register customer endpoint
app.MapPost(
    "/api/v1/customers",
    async (
        RegisterCustomerRequest request,
        RegisterCustomerUseCase useCase,
        CancellationToken cancellationToken) =>
    {
        try
        {
            var result = await useCase.ExecuteAsync(
                request,
                cancellationToken);

            if (!result.Succeeded)
            {
                return Results.Conflict(new
                {
                    error = result.Error
                });
            }

            return Results.Created(
                $"/api/v1/customers/{result.CustomerId}",
                new
                {
                    customerId = result.CustomerId
                });
        }
        catch (DomainException exception)
        {
            return Results.BadRequest(new
            {
                error = exception.Message
            });
        }
        catch (DbUpdateException exception)
            when (IsDuplicateCustomerEmail(exception))
        {
            return Results.Conflict(new
            {
                error = "A customer with this email address already exists."
            });
        }
    });

app.Run();

static bool IsDuplicateCustomerEmail(
    DbUpdateException exception)
{
    return exception.InnerException is PostgresException postgresException
        && postgresException.SqlState == PostgresErrorCodes.UniqueViolation
        && postgresException.ConstraintName == "ix_customers_email";
}

public partial class Program { }
