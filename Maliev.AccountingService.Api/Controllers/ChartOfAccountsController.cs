using Maliev.AccountingService.Api.DTOs.Requests;
using Maliev.AccountingService.Api.DTOs.Responses;
using Asp.Versioning;
using Maliev.AccountingService.Api.Services;
using Maliev.AccountingService.Data.Data;
using Maliev.Aspire.ServiceDefaults.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maliev.AccountingService.Api.Controllers;

/// <summary>
/// API endpoints for managing the chart of accounts
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("accounting/v{version:apiVersion}/chart-of-accounts")]
[Authorize]
public sealed class ChartOfAccountsController : ControllerBase
{
    private readonly IChartOfAccountsService _chartOfAccountsService;
    private readonly ILogger<ChartOfAccountsController> _logger;

    public ChartOfAccountsController(
        IChartOfAccountsService chartOfAccountsService,
        ILogger<ChartOfAccountsController> logger)
    {
        _chartOfAccountsService = chartOfAccountsService;
        _logger = logger;
    }

    /// <summary>
    /// Get all chart of accounts with optional filtering
    /// </summary>
    /// <param name="accountType">Optional filter by account type (Asset, Liability, Equity, Revenue, Expense)</param>
    /// <param name="includeInactive">Include deactivated accounts in results</param>
    /// <returns>List of chart of accounts</returns>
    [HttpGet]
    [RequirePermission(AccountingPermissions.AccountsRead)]
    [ProducesResponseType(typeof(List<ChartOfAccountResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ChartOfAccountResponse>>> GetAccounts(
        [FromQuery] string? accountType = null,
        [FromQuery] bool includeInactive = false)
    {
        _logger.LogInformation(
            "Getting chart of accounts with filters - Type: {Type}, IncludeInactive: {IncludeInactive}",
            accountType ?? "All",
            includeInactive);

        var accounts = await _chartOfAccountsService.GetAllAccountsAsync(accountType, includeInactive);

        return Ok(accounts);
    }

    /// <summary>
    /// Get chart of accounts hierarchy (parent-child tree structure)
    /// </summary>
    /// <param name="accountType">Optional filter by account type</param>
    /// <returns>Hierarchical list of chart of accounts</returns>
    [HttpGet("hierarchy")]
    [RequirePermission(AccountingPermissions.AccountsRead)]
    [ProducesResponseType(typeof(List<ChartOfAccountResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ChartOfAccountResponse>>> GetAccountHierarchy(
        [FromQuery] string? accountType = null)
    {
        _logger.LogInformation(
            "Getting chart of accounts hierarchy - Type: {Type}",
            accountType ?? "All");

        var hierarchy = await _chartOfAccountsService.GetAccountHierarchyAsync(accountType);

        return Ok(hierarchy);
    }

    /// <summary>
    /// Get a specific chart of account by ID
    /// </summary>
    /// <param name="id">Account ID</param>
    /// <returns>Chart of account details</returns>
    [HttpGet("{id:guid}")]
    [RequirePermission(AccountingPermissions.AccountsRead)]
    [ProducesResponseType(typeof(ChartOfAccountResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ChartOfAccountResponse>> GetAccountById(Guid id)
    {
        _logger.LogInformation("Getting chart of account by ID: {Id}", id);

        var account = await _chartOfAccountsService.GetAccountByIdAsync(id);

        if (account == null)
        {
            return NotFound(new { message = $"Account with ID '{id}' not found" });
        }

        return Ok(account);
    }

    /// <summary>
    /// Get a specific chart of account by account number
    /// </summary>
    /// <param name="accountNumber">Account number</param>
    /// <returns>Chart of account details</returns>
    [HttpGet("by-number/{accountNumber}")]
    [RequirePermission(AccountingPermissions.AccountsRead)]
    [ProducesResponseType(typeof(ChartOfAccountResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ChartOfAccountResponse>> GetAccountByNumber(string accountNumber)
    {
        _logger.LogInformation("Getting chart of account by number: {AccountNumber}", accountNumber);

        var account = await _chartOfAccountsService.GetAccountByNumberAsync(accountNumber);

        if (account == null)
        {
            return NotFound(new { message = $"Account with number '{accountNumber}' not found" });
        }

        return Ok(account);
    }

    /// <summary>
    /// Create a new chart of account
    /// </summary>
    /// <param name="request">Account creation details</param>
    /// <returns>Created chart of account</returns>
    [HttpPost]
    [RequirePermission(AccountingPermissions.AccountsCreate)]
    [ProducesResponseType(typeof(ChartOfAccountResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ChartOfAccountResponse>> CreateAccount(
        [FromBody] CreateChartOfAccountRequest request)
    {
        _logger.LogInformation(
            "Creating chart of account: {AccountNumber} - {Name}",
            request.AccountNumber,
            request.Name);

        try
        {
            var account = await _chartOfAccountsService.CreateAccountAsync(request);

            return CreatedAtAction(
                nameof(GetAccountById),
                new { id = account.Id },
                account);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to create chart of account: {Message}", ex.Message);
            return Conflict(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid account creation request: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing chart of account
    /// </summary>
    /// <param name="id">Account ID</param>
    /// <param name="request">Account update details</param>
    /// <returns>Updated chart of account</returns>
    [HttpPut("{id:guid}")]
    [RequirePermission(AccountingPermissions.AccountsUpdate)]
    [ProducesResponseType(typeof(ChartOfAccountResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ChartOfAccountResponse>> UpdateAccount(
        Guid id,
        [FromBody] UpdateChartOfAccountRequest request)
    {
        _logger.LogInformation("Updating chart of account: {Id}", id);

        try
        {
            var account = await _chartOfAccountsService.UpdateAccountAsync(id, request);
            return Ok(account);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            _logger.LogWarning(ex, "Account not found for update: {Id}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to update chart of account: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid account update request: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Deactivate a chart of account (soft delete)
    /// </summary>
    /// <param name="id">Account ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("{id:guid}")]
    [RequirePermission(AccountingPermissions.AccountsDelete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateAccount(Guid id)
    {
        _logger.LogInformation("Deactivating chart of account: {Id}", id);

        try
        {
            var result = await _chartOfAccountsService.DeactivateAccountAsync(id);

            if (!result)
            {
                return NotFound(new { message = $"Account with ID '{id}' not found" });
            }

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to deactivate chart of account: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Validate if an account can be deactivated
    /// </summary>
    /// <param name="id">Account ID</param>
    /// <returns>Validation result</returns>
    [HttpGet("{id:guid}/can-deactivate")]
    [RequirePermission(AccountingPermissions.AccountsRead)]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<ActionResult> ValidateDeactivation(Guid id)
    {
        _logger.LogInformation("Validating deactivation for chart of account: {Id}", id);

        var (isValid, errorMessage) = await _chartOfAccountsService.ValidateDeactivationAsync(id);

        return Ok(new
        {
            canDeactivate = isValid,
            reason = errorMessage
        });
    }
}
