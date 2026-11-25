using BankingPlatform.Infrastructure;
using BankingPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

// PostgreSQL configuration
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

app.Run();

public partial class Program { }
