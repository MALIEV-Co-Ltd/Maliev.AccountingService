using System.Net;
using Maliev.AccountingService.Api.Services;
using Maliev.AccountingService.Data.Models;
using Xunit;

namespace Maliev.AccountingService.Tests.Integration;

public class AuditIntegrationTests : BaseIntegrationTest
{
    private readonly IAuditService _auditService;

    public AuditIntegrationTests(IntegrationTestFixture fixture) : base(fixture)
    {
        _auditService = (IAuditService)Factory.Services.GetService(typeof(IAuditService))!;
    }

    [Fact]
    public async Task RecordAudit_ShouldSaveEntry()
    {
        await CleanDatabaseAsync();

        // Act
        await _auditService.RecordAuditAsync("TestEntity", "123", "Create", null, new { Name = "Test" }, Guid.NewGuid().ToString(), null, null);

        // Assert
        var dbContext = Factory.GetDbContext();
        var entry = dbContext.AuditTrailEntries.FirstOrDefault();
        Assert.NotNull(entry);
        Assert.Equal("TestEntity", entry.EntityType);
        Assert.Equal("123", entry.EntityId);
    }

    [Fact]
    public async Task GetAuditTrail_ShouldReturnEntries()
    {
        await CleanDatabaseAsync();

        // Arrange
        var entityId = "456";
        await _auditService.RecordAuditAsync("TestEntity", entityId, "Action1", null, null, null, null, null);
        await _auditService.RecordAuditAsync("TestEntity", entityId, "Action2", null, null, null, null, null);

        // Act
        var results = await _auditService.GetAuditTrailAsync("TestEntity", entityId);

        // Assert
        Assert.Equal(2, results.Count());
    }

    [Fact]
    public async Task GetAuditTrailByUser_ShouldReturnEntries()
    {
        await CleanDatabaseAsync();

        // Arrange
        var userId = Guid.NewGuid();
        await _auditService.RecordAuditAsync("Type", "Id", "Action", null, null, userId.ToString(), null, null);

        // Act
        var results = await _auditService.GetAuditTrailByUserAsync(userId.ToString());

        // Assert
        Assert.Single(results);
        Assert.Equal(userId, results.First().PerformedBy);
    }

    [Fact]
    public async Task GetAuditTrailByCorrelationId_ShouldReturnEntries()
    {
        await CleanDatabaseAsync();

        // Arrange
        var correlationId = "corr-123";
        await _auditService.RecordAuditAsync("Type", "Id", "Action", null, null, null, correlationId, null);

        // Act
        var results = await _auditService.GetAuditTrailByCorrelationIdAsync(correlationId);

        // Assert
        Assert.Single(results);
        Assert.Equal(correlationId, results.First().CorrelationId);
    }
}
