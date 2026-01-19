using Maliev.AccountingService.Api.Services;
using Maliev.Aspire.ServiceDefaults.Caching;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Maliev.AccountingService.Tests.Unit;

/// <summary>
/// Unit tests for event idempotency services
/// Tests both InMemory and Redis implementations
/// </summary>
public class EventIdempotencyServiceTests
{
    [Fact]
    public async Task InMemory_IsEventProcessed_ReturnsFalse_WhenEventNotProcessed()
    {
        // Arrange
        var service = new InMemoryEventIdempotencyService();
        var eventId = Guid.NewGuid().ToString();

        // Act
        var result = await service.IsEventProcessedAsync(eventId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task InMemory_IsEventProcessed_ReturnsTrue_AfterMarking()
    {
        // Arrange
        var service = new InMemoryEventIdempotencyService();
        var eventId = Guid.NewGuid().ToString();
        var journalEntryId = Guid.NewGuid();

        // Act
        await service.MarkEventAsProcessedAsync(eventId, journalEntryId);
        var result = await service.IsEventProcessedAsync(eventId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task InMemory_GetJournalEntryId_ReturnsNull_WhenNotProcessed()
    {
        // Arrange
        var service = new InMemoryEventIdempotencyService();
        var eventId = Guid.NewGuid().ToString();

        // Act
        var result = await service.GetJournalEntryIdAsync(eventId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task InMemory_GetJournalEntryId_ReturnsCorrectId_AfterMarking()
    {
        // Arrange
        var service = new InMemoryEventIdempotencyService();
        var eventId = Guid.NewGuid().ToString();
        var journalEntryId = Guid.NewGuid();

        // Act
        await service.MarkEventAsProcessedAsync(eventId, journalEntryId);
        var result = await service.GetJournalEntryIdAsync(eventId);

        // Assert
        Assert.Equal(journalEntryId, result);
    }

    [Fact]
    public async Task InMemory_MarkEventAsProcessed_CanBeCalledMultipleTimes()
    {
        // Arrange
        var service = new InMemoryEventIdempotencyService();
        var eventId = Guid.NewGuid().ToString();
        var journalEntryId1 = Guid.NewGuid();
        var journalEntryId2 = Guid.NewGuid();

        // Act
        await service.MarkEventAsProcessedAsync(eventId, journalEntryId1);
        await service.MarkEventAsProcessedAsync(eventId, journalEntryId2); // Overwrite

        var result = await service.GetJournalEntryIdAsync(eventId);

        // Assert - Should store the latest value
        Assert.Equal(journalEntryId2, result);
    }

    [Fact]
    public async Task InMemory_HandlesConcurrentAccess()
    {
        // Arrange
        var service = new InMemoryEventIdempotencyService();
        var tasks = new List<Task>();

        // Act - Mark 100 different events concurrently
        for (int i = 0; i < 100; i++)
        {
            var eventId = $"event-{i}";
            var journalEntryId = Guid.NewGuid();
            tasks.Add(service.MarkEventAsProcessedAsync(eventId, journalEntryId));
        }

        await Task.WhenAll(tasks);

        // Assert - All events should be marked
        for (int i = 0; i < 100; i++)
        {
            var eventId = $"event-{i}";
            var isProcessed = await service.IsEventProcessedAsync(eventId);
            Assert.True(isProcessed, $"Event {eventId} should be marked as processed");
        }
    }

    [Fact]
    public async Task Redis_IsEventProcessed_ReturnsFalse_WhenCacheEmpty()
    {
        // Arrange
        var mockCache = new Mock<ICacheService>();
        var mockLogger = new Mock<ILogger<RedisEventIdempotencyService>>();
        // It.IsAny<object>() doesn't work for generic return type T, need to mock the specific call or object
        // Since GetAsync is generic <T>, we need to setup for <object> or specific type used in service.
        // The service uses GetAsync<ProcessedEventData>. We can't access private class ProcessedEventData easily.
        // However, Moq can mock generic methods.

        // Reflection approach or making ProcessedEventData internal/public would be easier.
        // Assuming ProcessedEventData is private, we rely on Moq's loose behavior returning null for nullable types,
        // or we need to match the method call.

        // Actually, since ProcessedEventData is private inside RedisEventIdempotencyService, we can't use it in tests easily.
        // This is a design issue for testing.
        // However, we can mock the method using It.IsAnyType if we use a specific Moq setup,
        // or better, if the service class exposed the data type.

        // For now, let's assume we can match the call by checking if it returns null by default (Loose mock).
        // Default mock behavior for Task<T> is to return null (or Task.FromResult(default)).

        var service = new RedisEventIdempotencyService(mockCache.Object, mockLogger.Object);
        var eventId = Guid.NewGuid().ToString();

        // Act
        var result = await service.IsEventProcessedAsync(eventId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task Redis_MarkEventAsProcessed_ShouldSetCache()
    {
        // Arrange
        var mockCache = new Mock<ICacheService>();
        var mockLogger = new Mock<ILogger<RedisEventIdempotencyService>>();
        var service = new RedisEventIdempotencyService(mockCache.Object, mockLogger.Object);
        var eventId = "test-event";
        var journalEntryId = Guid.NewGuid();

        // Act
        await service.MarkEventAsProcessedAsync(eventId, journalEntryId);

        // Assert
        mockCache.Verify(c => c.SetAsync(
            It.Is<string>(s => s.Contains(eventId)),
            It.Is<RedisEventIdempotencyService.ProcessedEventData>(d => d.EventId == eventId && d.JournalEntryId == journalEntryId),
            It.IsAny<TimeSpan>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Redis_IsEventProcessed_ShouldReturnTrue_WhenCacheHasData()
    {
        // Arrange
        var mockCache = new Mock<ICacheService>();
        var mockLogger = new Mock<ILogger<RedisEventIdempotencyService>>();
        var eventId = "test-event";
        var data = new RedisEventIdempotencyService.ProcessedEventData { EventId = eventId, JournalEntryId = Guid.NewGuid() };

        mockCache.Setup(c => c.GetAsync<RedisEventIdempotencyService.ProcessedEventData>(
            It.Is<string>(s => s.Contains(eventId)),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(data);

        var service = new RedisEventIdempotencyService(mockCache.Object, mockLogger.Object);

        // Act
        var result = await service.IsEventProcessedAsync(eventId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task Redis_GetJournalEntryId_ShouldReturnId_WhenCacheHasData()
    {
        // Arrange
        var mockCache = new Mock<ICacheService>();
        var mockLogger = new Mock<ILogger<RedisEventIdempotencyService>>();
        var eventId = "test-event";
        var journalEntryId = Guid.NewGuid();
        var data = new RedisEventIdempotencyService.ProcessedEventData { EventId = eventId, JournalEntryId = journalEntryId };

        mockCache.Setup(c => c.GetAsync<RedisEventIdempotencyService.ProcessedEventData>(
            It.Is<string>(s => s.Contains(eventId)),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(data);

        var service = new RedisEventIdempotencyService(mockCache.Object, mockLogger.Object);

        // Act
        var result = await service.GetJournalEntryIdAsync(eventId);

        // Assert
        Assert.Equal(journalEntryId, result);
    }
}
