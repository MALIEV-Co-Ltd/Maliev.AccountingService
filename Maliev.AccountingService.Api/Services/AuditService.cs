using Maliev.AccountingService.Infrastructure.Data;
using Maliev.AccountingService.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Maliev.AccountingService.Api.Services;

/// <summary>
/// Implementation of audit trail service with append-only audit entries
/// </summary>
public class AuditService : IAuditService
{
    private readonly AccountingDbContext _context;
    private readonly ILogger<AuditService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuditService"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="logger">The logger.</param>
    public AuditService(
        AccountingDbContext context,
        ILogger<AuditService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Records an audit trail entry.
    /// </summary>
    /// <param name="entityType">The type of the audited entity.</param>
    /// <param name="entityId">The ID of the audited entity.</param>
    /// <param name="action">The action performed.</param>
    /// <param name="beforeState">The state before the operation.</param>
    /// <param name="afterState">The state after the operation.</param>
    /// <param name="userId">The ID of the user who performed the operation.</param>
    /// <param name="correlationId">The correlation ID for the operation.</param>
    /// <param name="ipAddress">The IP address of the user.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task RecordAuditAsync(
        string entityType,
        string entityId,
        string action,
        object? beforeState,
        object? afterState,
        string? userId,
        string? correlationId,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var jsonOptions = new JsonSerializerOptions
        {
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles,
            WriteIndented = false
        };

        var auditEntry = new AuditTrailEntry
        {
            Id = Guid.NewGuid(),
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            BeforeSnapshot = beforeState != null ? JsonSerializer.Serialize(beforeState, jsonOptions) : null,
            AfterSnapshot = afterState != null ? JsonSerializer.Serialize(afterState, jsonOptions) : null,
            PerformedBy = userId != null && Guid.TryParse(userId, out var userGuid) ? userGuid : Guid.Empty,
            CorrelationId = correlationId,
            IpAddress = ipAddress,
            PerformedAt = DateTime.UtcNow
        };

        _context.AuditTrailEntries.Add(auditEntry);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Audit trail recorded for {EntityType} {EntityId}: {Action} by user {UserId}",
            entityType,
            entityId,
            action,
            userId ?? "system");
    }

    /// <summary>
    /// Retrieves the audit trail for a specific entity.
    /// </summary>
    /// <param name="entityType">The type of the audited entity.</param>
    /// <param name="entityId">The ID of the audited entity.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of audit trail entries.</returns>
    public async Task<IEnumerable<AuditTrailEntry>> GetAuditTrailAsync(
        string entityType,
        string entityId,
        CancellationToken cancellationToken = default)
    {
        return await _context.AuditTrailEntries
            .Where(a => a.EntityType == entityType && a.EntityId == entityId)
            .OrderBy(a => a.PerformedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves the audit trail for a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of audit trail entries.</returns>
    public async Task<IEnumerable<AuditTrailEntry>> GetAuditTrailByUserAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var userGuid = Guid.Parse(userId);
        return await _context.AuditTrailEntries
            .Where(a => a.PerformedBy == userGuid)
            .OrderBy(a => a.PerformedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves the audit trail for a specific correlation ID.
    /// </summary>
    /// <param name="correlationId">The correlation ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of audit trail entries.</returns>
    public async Task<IEnumerable<AuditTrailEntry>> GetAuditTrailByCorrelationIdAsync(
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        return await _context.AuditTrailEntries
            .Where(a => a.CorrelationId == correlationId)
            .OrderBy(a => a.PerformedAt)
            .ToListAsync(cancellationToken);
    }
}
