using Maliev.AccountingService.Infrastructure.Models;

namespace Maliev.AccountingService.Api.Services;

/// <summary>
/// Service for creating append-only audit trail entries
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Records an audit trail entry for an entity change
    /// </summary>
    /// <param name="entityType">Type of entity being modified</param>
    /// <param name="entityId">Unique identifier of the entity</param>
    /// <param name="action">Action performed (Created, Modified, Deleted, Posted, etc.)</param>
    /// <param name="beforeState">State before modification (null for creation)</param>
    /// <param name="afterState">State after modification (null for deletion)</param>
    /// <param name="userId">User who performed the action</param>
    /// <param name="correlationId">Correlation ID for distributed tracing</param>
    /// <param name="ipAddress">IP address of the request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RecordAuditAsync(
        string entityType,
        string entityId,
        string action,
        object? beforeState,
        object? afterState,
        string? userId,
        string? correlationId,
        string? ipAddress,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves audit trail entries for a specific entity
    /// </summary>
    /// <param name="entityType">Type of entity</param>
    /// <param name="entityId">Entity identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of audit trail entries ordered by timestamp</returns>
    Task<IEnumerable<AuditTrailEntry>> GetAuditTrailAsync(
        string entityType,
        string entityId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves audit trail entries by user
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of audit trail entries ordered by timestamp</returns>
    Task<IEnumerable<AuditTrailEntry>> GetAuditTrailByUserAsync(
        string userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves audit trail entries by correlation ID
    /// </summary>
    /// <param name="correlationId">Correlation ID from distributed trace</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of audit trail entries ordered by timestamp</returns>
    Task<IEnumerable<AuditTrailEntry>> GetAuditTrailByCorrelationIdAsync(
        string correlationId,
        CancellationToken cancellationToken = default);
}
