using Microsoft.EntityFrameworkCore;
using MassTransit;
using Maliev.AccountingService.Api;
using Maliev.AccountingService.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Configure PostgreSQL DbContext
var connectionString = builder.Configuration.GetConnectionString("ServiceDbContext")
    ?? throw new InvalidOperationException("Connection string 'ServiceDbContext' not found.");

builder.Services.AddDbContext<AccountingDbContext>(options =>
    options.UseNpgsql(connectionString));

// Configure MassTransit with RabbitMQ
builder.Services.AddMassTransit(x =>
{
    // Register consumers here when created
    // x.AddConsumer<InvoiceCreatedConsumer>();
    // x.AddConsumer<PaymentReceivedConsumer>();
    // etc.

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitMqConnection = builder.Configuration.GetConnectionString("RabbitMQ")
            ?? "amqp://guest:guest@localhost:5672/";
        cfg.Host(new Uri(rabbitMqConnection));

        cfg.ConfigureEndpoints(context);
    });
});

// Configure health checks
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString, name: "database");

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Register application services here when created
// builder.Services.AddScoped<IEventProcessingService, EventProcessingService>();
// builder.Services.AddScoped<IEventIdempotencyService, RedisEventIdempotencyService>();
// builder.Services.AddScoped<IAuditService, AuditService>();
// etc.

var app = builder.Build();

// Configure the HTTP request pipeline

// Exception handling middleware (must be first)
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthorization();

app.MapControllers();

// Map health check endpoints
app.MapHealthChecks("/accounting/health");
app.MapHealthChecks("/accounting/liveness", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false
});

app.Run();
