using Maliev.AccountingService.Api.Middleware;
using Maliev.AccountingService.Data.Data;

var builder = WebApplication.CreateBuilder(args);

// --- Secrets & Configuration ---
builder.AddGoogleSecretManagerVolume(); // Load secrets from /mnt/secrets if available

// --- Infrastructure & Observability ---
builder.AddServiceDefaults(); // OpenTelemetry, health checks, resilience
builder.AddServiceMeters("accounting-meter"); // Register service meters for OpenTelemetry business metrics

// Database Context with ServiceDefaults
builder.AddPostgresDbContext<AccountingDbContext>(
    connectionStringName: "AccountingDbContext");

builder.AddRedisDistributedCache(instanceName: "accounting:"); // Redis with in-memory fallback
builder.AddMassTransitWithRabbitMq(x =>
{
    // Register all event consumers
    x.AddConsumer<Maliev.AccountingService.Api.Consumers.InvoiceCreatedConsumer>();
    x.AddConsumer<Maliev.AccountingService.Api.Consumers.PaymentReceivedConsumer>();
    x.AddConsumer<Maliev.AccountingService.Api.Consumers.SupplierInvoiceConsumer>();
    x.AddConsumer<Maliev.AccountingService.Api.Consumers.InventoryMovementConsumer>();
    x.AddConsumer<Maliev.AccountingService.Api.Consumers.PayrollProcessedConsumer>();
}); // RabbitMQ message bus (non-blocking startup)

// --- API Configuration ---
builder.AddDefaultCors(); // CORS from CORS:AllowedOrigins config
builder.AddDefaultApiVersioning(); // API versioning with URL segment reader

// JWT Authentication (tests override via PostConfigureAll with dynamic RSA keys)
builder.AddJwtAuthentication();

// Add OpenAPI (must be in Program.cs for XML comments to work via source generator)
if (!builder.Environment.IsProduction())
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddOpenApi("v1", options =>
    {
        options.AddDocumentTransformer((document, context, cancellationToken) =>
        {
            document.Info.Title = "MALIEV Accounting Service API";
            document.Info.Version = "v1";
            document.Info.Description = "Double-entry accounting service. Handles chart of accounts, journal entries, general ledger, financial periods, audit trails, reconciliations, and event-sourced transaction processing from other services.";
            return Task.CompletedTask;
        });
    });
}

builder.Services.AddControllers();
builder.Services.AddMemoryCache();

// Register application services
builder.Services.AddScoped<Maliev.AccountingService.Api.Services.IEventProcessingService,
    Maliev.AccountingService.Api.Services.EventProcessingService>();
builder.Services.AddScoped<Maliev.AccountingService.Api.Services.IEventIdempotencyService,
    Maliev.AccountingService.Api.Services.RedisEventIdempotencyService>();
builder.Services.AddScoped<Maliev.AccountingService.Api.Services.IAuditService,
    Maliev.AccountingService.Api.Services.AuditService>();
builder.Services.AddScoped<Maliev.AccountingService.Api.Services.IChartOfAccountsService,
    Maliev.AccountingService.Api.Services.ChartOfAccountsService>();

// Register metrics
builder.Services.AddSingleton<Maliev.AccountingService.Api.Metrics.AccountingMetrics>();

var app = builder.Build();
var logger = app.Services.GetRequiredService<ILogger<Program>>();

// Run database migrations on startup (skip in Testing environment)
if (!app.Environment.IsEnvironment("Testing"))
{
    try
    {
        await app.MigrateDatabaseAsync<AccountingDbContext>();
    }
    catch (Exception ex)
    {
        Log.MigrationFailed(logger, ex);
        // Don't throw - allow app to start for debugging
    }
}

// Middleware Pipeline
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

// Map endpoints after middleware
app.MapControllers();

// Map Aspire default endpoints (/health, /alive, /metrics)
app.MapDefaultEndpoints(servicePrefix: "accounting");

// Map OpenAPI and Scalar documentation (dev/staging only)
app.MapApiDocumentation(servicePrefix: "accounting");

Log.ServiceStarted(logger);
await app.RunAsync();

/// <summary>
/// Main program class for the application
/// </summary>
public partial class Program
{
    internal static partial class Log
    {
        [LoggerMessage(Level = LogLevel.Information, Message = "AccountingService started successfully")]
        public static partial void ServiceStarted(ILogger logger);

        [LoggerMessage(Level = LogLevel.Error, Message = "Database migration failed - application may not function correctly")]
        public static partial void MigrationFailed(ILogger logger, Exception exception);
    }
}
