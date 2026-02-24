using Maliev.AccountingService.Api.Services;
using Maliev.AccountingService.Data.Data;
using Maliev.Aspire.ServiceDefaults;

// Initialize bootstrap logging
using var loggerFactory = LoggerFactory.Create(logBuilder => logBuilder.AddConsole());
var bootstrapLogger = loggerFactory.CreateLogger("Program");

try
{
    Program.Log.StartingHost(bootstrapLogger, "Accounting Service");

    var builder = WebApplication.CreateBuilder(args);

    // --- Secrets & Configuration ---
    builder.AddGoogleSecretManagerVolume(); // Load secrets from /mnt/secrets if available

    // --- Infrastructure & Observability ---
    builder.AddServiceDefaults(); // OpenTelemetry, health checks, resilience
    builder.AddStandardMiddleware(options =>
    {
        options.EnableRequestLogging = true;
    });
    builder.AddServiceMeters("accounting-meter"); // Register service meters for OpenTelemetry business metrics

    builder.Services.AddHttpContextAccessor();

    // Database Context with ServiceDefaults
    builder.AddPostgresDbContext<AccountingDbContext>(
        connectionName: "AccountingDbContext");

    builder.AddStandardCache("accounting:"); // Redis + in-memory fallback, memory-optimized // Redis with in-memory fallback
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
    builder.AddStandardCors(); // CORS with fail-fast validation
    builder.AddDefaultApiVersioning(); // API versioning with URL segment reader

    // JWT Authentication (tests override via PostConfigureAll with dynamic RSA keys)
    builder.AddJwtAuthentication();

    // Add OpenAPI (must be in Program.cs for XML comments to work via source generator)
    if (!builder.Environment.IsProduction())
    {
        builder.AddStandardOpenApi(
            title: "MALIEV Accounting Service API",
            description: "Double-entry accounting service. Handles chart of accounts, journal entries, general ledger, financial periods, audit trails, reconciliations, and event-sourced transaction processing from other services.");
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
    builder.Services.AddScoped<IChartOfAccountsService,
        Maliev.AccountingService.Api.Services.ChartOfAccountsService>();
    builder.Services.AddScoped<Maliev.AccountingService.Api.Services.IReportingService,
        Maliev.AccountingService.Api.Services.ReportingService>();
    builder.Services.AddScoped<Maliev.AccountingService.Api.Services.IReconciliationService,
        Maliev.AccountingService.Api.Services.ReconciliationService>();
    builder.Services.AddScoped<Maliev.AccountingService.Api.Services.ITaxCalculationService,
        Maliev.AccountingService.Api.Services.TaxCalculationService>();
    builder.Services.AddScoped<Maliev.AccountingService.Api.Services.IBulkImportService,
        Maliev.AccountingService.Api.Services.BulkImportService>();
    builder.Services.AddScoped<Maliev.AccountingService.Api.Services.IPeriodService,
        Maliev.AccountingService.Api.Services.PeriodService>();

    // Authorization Infrastructure
    builder.Services.AddPermissionAuthorization();

    // IAM Registration
    builder.AddIAMServiceClient("accounting");
    builder.Services.AddIAMRegistration<AccountingIAMRegistrationService>("accounting");

    // Register metrics
    builder.Services.AddSingleton<Maliev.AccountingService.Api.Metrics.AccountingMetrics>();

    var app = builder.Build();
    var logger = app.Services.GetRequiredService<ILogger<Program>>();

    // --- Database Migrations ---
    await app.MigrateDatabaseAsync<AccountingDbContext>();

    // Middleware Pipeline
    app.UseStandardMiddleware();

    if (!app.Environment.IsDevelopment())
    {
        app.UseHttpsRedirection();
    }
    app.UseCors();

    app.UseAuthentication();
    app.UseAuthorization();

    // Map endpoints after middleware
    app.MapControllers();

    // Map Aspire default endpoints (/health, /alive, /metrics)
    app.MapDefaultEndpoints(servicePrefix: "accounting");

    // Map OpenAPI and Scalar documentation (dev/staging only)
    app.MapApiDocumentation(servicePrefix: "accounting");

    Program.Log.ServiceStarted(logger, "Accounting Service");
    await app.RunAsync();
}
catch (Exception ex)
{
    Program.Log.HostTerminated(bootstrapLogger, ex, "Accounting Service");
    throw;
}
finally
{
    loggerFactory.Dispose();
}

/// <summary>
/// Main program class for the application
/// </summary>
public partial class Program
{
    internal static partial class Log
    {
        [LoggerMessage(Level = LogLevel.Information, Message = "Starting {ServiceName} host")]
        public static partial void StartingHost(ILogger logger, string serviceName);

        [LoggerMessage(Level = LogLevel.Critical, Message = "{ServiceName} host terminated unexpectedly during startup")]
        public static partial void HostTerminated(ILogger logger, Exception ex, string serviceName);

        [LoggerMessage(Level = LogLevel.Information, Message = "{ServiceName} started successfully")]
        public static partial void ServiceStarted(ILogger logger, string serviceName);

        [LoggerMessage(Level = LogLevel.Error, Message = "Database migration failed - application may not function correctly")]
        public static partial void MigrationFailed(ILogger logger, Exception exception);
    }
}
