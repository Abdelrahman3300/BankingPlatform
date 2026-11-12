
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapHealthChecks("/health");

app.MapGet("/", () =>
{
    return Results.Ok(new
    {
        application = "BankingPlatform",
        status = "Running",
        version = "1.0.0"
    });
});

app.MapGet("/api/v1/status", () =>
{
    return Results.Ok(new
    {
        status = "OK",
        message = "BankingPlatform API is running"
    });
});

app.Run();

public partial class Program { }
