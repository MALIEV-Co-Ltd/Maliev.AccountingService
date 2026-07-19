namespace Maliev.AccountingService.Api.Services;

/// <summary>
/// Service for tracking processed events to ensure idempotency
/// </summary>
public interface IEventIdempotencyService
{
    /// <summary>
    /// Checks if an event has already been processed
    /// </summary>
    /// <param name="eventId">The unique event identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the event has been processed, false otherwise</returns>
    Task<bool> IsEventProcessedAsync(string eventId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an event as processed
    /// </summary>
    /// <param name="eventId">The unique event identifier</param>
    /// <param name="journalEntryId">The associated journal entry ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task MarkEventAsProcessedAsync(string eventId, Guid journalEntryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the journal entry ID associated with a processed event
    /// </summary>
    /// <param name="eventId">The unique event identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The journal entry ID if found, null otherwise</returns>
    Task<Guid?> GetJournalEntryIdAsync(string eventId, CancellationToken cancellationToken = default);
}
