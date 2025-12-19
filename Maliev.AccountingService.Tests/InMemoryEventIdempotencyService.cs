using Maliev.AccountingService.Api.Services;
using System.Collections.Concurrent;

namespace Maliev.AccountingService.Tests;

/// <summary>
/// In-memory implementation of event idempotency service for testing
/// </summary>
public class InMemoryEventIdempotencyService : IEventIdempotencyService
{
    private readonly ConcurrentDictionary<string, Guid> _processedEvents = new();

    public Task<bool> IsEventProcessedAsync(string eventId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_processedEvents.ContainsKey(eventId));
    }

    public Task<Guid?> GetJournalEntryIdAsync(string eventId, CancellationToken cancellationToken = default)
    {
        if (_processedEvents.TryGetValue(eventId, out var journalEntryId))
        {
            return Task.FromResult<Guid?>(journalEntryId);
        }
        return Task.FromResult<Guid?>(null);
    }

    public Task MarkEventAsProcessedAsync(string eventId, Guid journalEntryId, CancellationToken cancellationToken = default)
    {
        _processedEvents[eventId] = journalEntryId;
        return Task.CompletedTask;
    }
}
