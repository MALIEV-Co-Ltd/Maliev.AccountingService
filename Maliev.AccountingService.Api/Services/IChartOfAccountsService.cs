using Maliev.AccountingService.Api.DTOs.Requests;
using Maliev.AccountingService.Api.DTOs.Responses;

namespace Maliev.AccountingService.Api.Services;

/// <summary>
/// Service interface for chart of accounts management
/// </summary>
public interface IChartOfAccountsService
{
    /// <summary>
    /// Get all accounts with optional type filtering
    /// </summary>
    Task<List<ChartOfAccountResponse>> GetAllAccountsAsync(string? accountType = null, bool includeInactive = false);

    /// <summary>
    /// Get account hierarchy (parent-child relationships)
    /// </summary>
    Task<List<ChartOfAccountResponse>> GetAccountHierarchyAsync(string? accountType = null);

    /// <summary>
    /// Get account by ID
    /// </summary>
    Task<ChartOfAccountResponse?> GetAccountByIdAsync(Guid id);

    /// <summary>
    /// Get account by account number
    /// </summary>
    Task<ChartOfAccountResponse?> GetAccountByNumberAsync(string accountNumber);

    /// <summary>
    /// Create a new account
    /// </summary>
    Task<ChartOfAccountResponse> CreateAccountAsync(CreateChartOfAccountRequest request);

    /// <summary>
    /// Update an existing account
    /// </summary>
    Task<ChartOfAccountResponse> UpdateAccountAsync(Guid id, UpdateChartOfAccountRequest request);

    /// <summary>
    /// Deactivate an account (soft delete)
    /// </summary>
    Task<bool> DeactivateAccountAsync(Guid id);

    /// <summary>
    /// Validate that an account can be deactivated
    /// </summary>
    Task<(bool IsValid, string? ErrorMessage)> ValidateDeactivationAsync(Guid id);
}
