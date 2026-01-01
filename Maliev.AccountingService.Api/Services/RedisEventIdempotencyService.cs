using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Maliev.AccountingService.Api.Services;

/// <summary>
/// Redis-based implementation of event idempotency tracking with 24h TTL
/// </summary>
public class RedisEventIdempotencyService : IEventIdempotencyService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<RedisEventIdempotencyService> _logger;
    private const string KeyPrefix = "event:processed:";
    private static readonly TimeSpan Ttl = TimeSpan.FromHours(24);

    /// <summary>
    /// Initializes a new instance of the <see cref="RedisEventIdempotencyService"/> class.
    /// </summary>
    /// <param name="cache">The distributed cache.</param>
    /// <param name="logger">The logger.</param>
    public RedisEventIdempotencyService(
        IDistributedCache cache,
        ILogger<RedisEventIdempotencyService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Checks if an event has already been processed.
    /// </summary>
    /// <param name="eventId">The event ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the event was processed; otherwise, false.</returns>
    public async Task<bool> IsEventProcessedAsync(string eventId, CancellationToken cancellationToken = default)
    {
        var key = GetKey(eventId);
        var value = await _cache.GetStringAsync(key, cancellationToken);

        var isProcessed = !string.IsNullOrEmpty(value);

        if (isProcessed)
        {
            _logger.LogDebug("Event {EventId} has already been processed", eventId);
        }

        return isProcessed;
    }

    /// <summary>
    /// Marks an event as processed.
    /// </summary>
    /// <param name="eventId">The event ID.</param>
    /// <param name="journalEntryId">The ID of the created journal entry.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task MarkEventAsProcessedAsync(string eventId, Guid journalEntryId, CancellationToken cancellationToken = default)
    {
        var key = GetKey(eventId);
        var data = new ProcessedEventData
        {
            EventId = eventId,
            JournalEntryId = journalEntryId,
            ProcessedAt = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(data);
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = Ttl
        };

        await _cache.SetStringAsync(key, json, options, cancellationToken);

        _logger.LogInformation(
            "Marked event {EventId} as processed with journal entry {JournalEntryId}",
            eventId,
            journalEntryId);
    }

    /// <summary>
    /// Retrieves the journal entry ID associated with a processed event.
    /// </summary>
    /// <param name="eventId">The event ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The journal entry ID if found; otherwise, null.</returns>
    public async Task<Guid?> GetJournalEntryIdAsync(string eventId, CancellationToken cancellationToken = default)
    {
        var key = GetKey(eventId);
        var json = await _cache.GetStringAsync(key, cancellationToken);

        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        var data = JsonSerializer.Deserialize<ProcessedEventData>(json);
        return data?.JournalEntryId;
    }

    private static string GetKey(string eventId) => $"{KeyPrefix}{eventId}";

    private class ProcessedEventData
    {
        public string EventId { get; set; } = string.Empty;
        public Guid JournalEntryId { get; set; }
        public DateTime ProcessedAt { get; set; }
    }
}
