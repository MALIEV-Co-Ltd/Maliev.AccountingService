using Maliev.AccountingService.Api.Services;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text;
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
        var mockCache = new Mock<IDistributedCache>();
        var mockLogger = new Mock<ILogger<RedisEventIdempotencyService>>();
        mockCache.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        var service = new RedisEventIdempotencyService(mockCache.Object, mockLogger.Object);
        var eventId = Guid.NewGuid().ToString();

        // Act
        var result = await service.IsEventProcessedAsync(eventId);

        // Assert
        Assert.False(result);
        mockCache.Verify(c => c.GetAsync(
            It.Is<string>(s => s.Contains(eventId)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Redis_IsEventProcessed_ReturnsTrue_WhenCacheContainsEvent()
    {
        // Arrange
        var mockCache = new Mock<IDistributedCache>();
        var mockLogger = new Mock<ILogger<RedisEventIdempotencyService>>();
        var journalEntryId = Guid.NewGuid();
        var cachedData = $"{{\"EventId\":\"test-event\",\"JournalEntryId\":\"{journalEntryId}\",\"ProcessedAt\":\"2025-01-01T00:00:00Z\"}}";
        var cachedBytes = Encoding.UTF8.GetBytes(cachedData);

        mockCache.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedBytes);

        var service = new RedisEventIdempotencyService(mockCache.Object, mockLogger.Object);

        // Act
        var result = await service.IsEventProcessedAsync("test-event");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task Redis_MarkEventAsProcessed_StoresInCache()
    {
        // Arrange
        var mockCache = new Mock<IDistributedCache>();
        var mockLogger = new Mock<ILogger<RedisEventIdempotencyService>>();

        mockCache.Setup(c => c.SetAsync(
            It.IsAny<string>(),
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = new RedisEventIdempotencyService(mockCache.Object, mockLogger.Object);

        var eventId = Guid.NewGuid().ToString();
        var journalEntryId = Guid.NewGuid();

        // Act
        await service.MarkEventAsProcessedAsync(eventId, journalEntryId);

        // Assert
        mockCache.Verify(c => c.SetAsync(
            It.Is<string>(s => s.Contains(eventId)),
            It.Is<byte[]>(bytes => Encoding.UTF8.GetString(bytes).Contains(journalEntryId.ToString())),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Redis_MarkEventAsProcessed_SetsExpirationTime()
    {
        // Arrange
        var mockCache = new Mock<IDistributedCache>();
        var mockLogger = new Mock<ILogger<RedisEventIdempotencyService>>();

        // Setup the SetAsync to not throw
        mockCache.Setup(c => c.SetAsync(
            It.IsAny<string>(),
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = new RedisEventIdempotencyService(mockCache.Object, mockLogger.Object);

        var eventId = Guid.NewGuid().ToString();
        var journalEntryId = Guid.NewGuid();

        // Act
        await service.MarkEventAsProcessedAsync(eventId, journalEntryId);

        // Assert - Verify TTL is set (24 hours)
        mockCache.Verify(c => c.SetAsync(
            It.IsAny<string>(),
            It.IsAny<byte[]>(),
            It.Is<DistributedCacheEntryOptions>(opts =>
                opts.AbsoluteExpirationRelativeToNow.HasValue &&
                opts.AbsoluteExpirationRelativeToNow.Value.TotalHours >= 23 &&
                opts.AbsoluteExpirationRelativeToNow.Value.TotalHours <= 25),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Redis_GetJournalEntryId_ReturnsNull_WhenNotInCache()
    {
        // Arrange
        var mockCache = new Mock<IDistributedCache>();
        var mockLogger = new Mock<ILogger<RedisEventIdempotencyService>>();
        mockCache.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        var service = new RedisEventIdempotencyService(mockCache.Object, mockLogger.Object);

        // Act
        var result = await service.GetJournalEntryIdAsync("test-event");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Redis_GetJournalEntryId_ReturnsCorrectId_WhenInCache()
    {
        // Arrange
        var mockCache = new Mock<IDistributedCache>();
        var mockLogger = new Mock<ILogger<RedisEventIdempotencyService>>();
        var journalEntryId = Guid.NewGuid();
        var cachedData = $"{{\"EventId\":\"test-event\",\"JournalEntryId\":\"{journalEntryId}\",\"ProcessedAt\":\"2025-01-01T00:00:00Z\"}}";
        var cachedBytes = Encoding.UTF8.GetBytes(cachedData);

        mockCache.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedBytes);

        var service = new RedisEventIdempotencyService(mockCache.Object, mockLogger.Object);

        // Act
        var result = await service.GetJournalEntryIdAsync("test-event");

        // Assert
        Assert.Equal(journalEntryId, result);
    }

    [Fact]
    public async Task Redis_GetJournalEntryId_HandlesInvalidJson()
    {
        // Arrange
        var mockCache = new Mock<IDistributedCache>();
        var mockLogger = new Mock<ILogger<RedisEventIdempotencyService>>();
        var invalidBytes = Encoding.UTF8.GetBytes("invalid json");

        mockCache.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(invalidBytes);

        var service = new RedisEventIdempotencyService(mockCache.Object, mockLogger.Object);

        // Act & Assert - Should handle gracefully (may throw or return null)
        try
        {
            var result = await service.GetJournalEntryIdAsync("test-event");
            // If it doesn't throw, result should be null
            Assert.Null(result);
        }
        catch (System.Text.Json.JsonException)
        {
            // Expected - invalid JSON
            Assert.True(true);
        }
    }

    [Fact]
    public async Task Redis_UsesCorrectKeyPrefix()
    {
        // Arrange
        var mockCache = new Mock<IDistributedCache>();
        var mockLogger = new Mock<ILogger<RedisEventIdempotencyService>>();

        mockCache.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        var service = new RedisEventIdempotencyService(mockCache.Object, mockLogger.Object);

        var eventId = "my-event-123";

        // Act
        await service.IsEventProcessedAsync(eventId);

        // Assert - Key should have prefix
        mockCache.Verify(c => c.GetAsync(
            It.Is<string>(s => s.StartsWith("event:processed:") && s.Contains(eventId)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Redis_HandlesCancellationToken()
    {
        // Arrange
        var mockCache = new Mock<IDistributedCache>();
        var mockLogger = new Mock<ILogger<RedisEventIdempotencyService>>();

        mockCache.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        var service = new RedisEventIdempotencyService(mockCache.Object, mockLogger.Object);
        var cts = new CancellationTokenSource();

        // Act
        await service.IsEventProcessedAsync("test-event", cts.Token);

        // Assert - Cancellation token was passed through
        mockCache.Verify(c => c.GetAsync(
            It.IsAny<string>(),
            It.Is<CancellationToken>(ct => ct == cts.Token)), Times.Once);
    }
}
