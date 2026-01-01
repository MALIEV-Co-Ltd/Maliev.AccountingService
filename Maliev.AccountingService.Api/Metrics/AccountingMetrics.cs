using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.Configuration;

namespace Maliev.AccountingService.Api.Metrics;

/// <summary>
/// Provides metrics for accounting service operations
/// </summary>
public class AccountingMetrics
{
    private readonly Meter _meter;
    private readonly Counter<long> _eventIngestionCounter;
    private readonly Histogram<double> _processingLatencyHistogram;
    private readonly Counter<long> _transactionProcessingCounter;
    private readonly Counter<long> _processingErrorCounter;
    private readonly KeyValuePair<string, object?>[] _defaultTags;

    /// <summary>
    /// Initializes a new instance of the <see cref="AccountingMetrics"/> class.
    /// </summary>
    /// <param name="meterFactory">The meter factory.</param>
    /// <param name="configuration">The configuration.</param>
    public AccountingMetrics(IMeterFactory meterFactory, IConfiguration configuration)
    {
        var serviceName = configuration["Service:Name"] ?? "accounting-service";
        _meter = meterFactory.Create($"{serviceName.ToLower()}-meter");

        _defaultTags = new[]
        {
            new KeyValuePair<string, object?>("service_name", serviceName),
            new KeyValuePair<string, object?>("version", configuration["Service:Version"] ?? "1.0.0"),
            new KeyValuePair<string, object?>("region", configuration["Service:Region"] ?? "global"),
            new KeyValuePair<string, object?>("environment", configuration["ASPNETCORE_ENVIRONMENT"] ?? "Development")
        };

        // Event ingestion counter - tracks incoming events by type
        _eventIngestionCounter = _meter.CreateCounter<long>(
            name: "event_ingestion_count",
            unit: "events",
            description: "Number of financial events ingested from message queues");

        // Processing latency histogram - tracks end-to-end processing time
        _processingLatencyHistogram = _meter.CreateHistogram<double>(
            name: "processing_latency_ms",
            unit: "milliseconds",
            description: "Time taken to process events and create journal entries");

        // Transaction processing rate counter
        _transactionProcessingCounter = _meter.CreateCounter<long>(
            name: "transaction_processing_rate",
            unit: "transactions",
            description: "Number of journal entries successfully created");

        // Processing error counter
        _processingErrorCounter = _meter.CreateCounter<long>(
            name: "processing_error_count",
            unit: "errors",
            description: "Number of event processing errors");
    }

    /// <summary>
    /// Record an ingested event
    /// </summary>
    public void RecordEventIngestion(string eventType, string sourceSystem)
    {
        var tags = new TagList();
        foreach (var tag in _defaultTags) tags.Add(tag);
        tags.Add("event.type", eventType);
        tags.Add("source.system", sourceSystem);
        _eventIngestionCounter.Add(1, tags);
    }

    /// <summary>
    /// Record processing latency
    /// </summary>
    public void RecordProcessingLatency(double latencyMs, string eventType, bool success)
    {
        var tags = new TagList();
        foreach (var tag in _defaultTags) tags.Add(tag);
        tags.Add("event.type", eventType);
        tags.Add("success", success);
        _processingLatencyHistogram.Record(latencyMs, tags);
    }

    /// <summary>
    /// Record successful transaction processing
    /// </summary>
    public void RecordTransactionProcessed(string eventType, decimal amount)
    {
        var tags = new TagList();
        foreach (var tag in _defaultTags) tags.Add(tag);
        tags.Add("event.type", eventType);
        tags.Add("amount", amount);
        _transactionProcessingCounter.Add(1, tags);
    }

    /// <summary>
    /// Record processing error
    /// </summary>
    public void RecordProcessingError(string eventType, string errorType)
    {
        var tags = new TagList();
        foreach (var tag in _defaultTags) tags.Add(tag);
        tags.Add("event.type", eventType);
        tags.Add("error.type", errorType);
        _processingErrorCounter.Add(1, tags);
    }
}
