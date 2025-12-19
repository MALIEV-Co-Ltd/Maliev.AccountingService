using System.Diagnostics.Metrics;

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

    public AccountingMetrics(IMeterFactory meterFactory)
    {
        _meter = meterFactory.Create("Maliev.AccountingService");

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
        _eventIngestionCounter.Add(1,
            new KeyValuePair<string, object?>("event.type", eventType),
            new KeyValuePair<string, object?>("source.system", sourceSystem));
    }

    /// <summary>
    /// Record processing latency
    /// </summary>
    public void RecordProcessingLatency(double latencyMs, string eventType, bool success)
    {
        _processingLatencyHistogram.Record(latencyMs,
            new KeyValuePair<string, object?>("event.type", eventType),
            new KeyValuePair<string, object?>("success", success));
    }

    /// <summary>
    /// Record successful transaction processing
    /// </summary>
    public void RecordTransactionProcessed(string eventType, decimal amount)
    {
        _transactionProcessingCounter.Add(1,
            new KeyValuePair<string, object?>("event.type", eventType),
            new KeyValuePair<string, object?>("amount", amount));
    }

    /// <summary>
    /// Record processing error
    /// </summary>
    public void RecordProcessingError(string eventType, string errorType)
    {
        _processingErrorCounter.Add(1,
            new KeyValuePair<string, object?>("event.type", eventType),
            new KeyValuePair<string, object?>("error.type", errorType));
    }
}
