namespace Maliev.AccountingService.Api.DTOs.Responses;

/// <summary>
/// Response DTO for chart of account information
/// </summary>
public sealed record ChartOfAccountResponse
{
    /// <summary>
    /// Gets the unique account ID.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the unique account number.
    /// </summary>
    public string AccountNumber { get; init; } = string.Empty;

    /// <summary>
    /// Gets the account name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the account description.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets the account type (Asset, Liability, Equity, Revenue, Expense).
    /// </summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>
    /// Gets the account category.
    /// </summary>
    public string? Category { get; init; }

    /// <summary>
    /// Gets the ID of the parent account.
    /// </summary>
    public Guid? ParentAccountId { get; init; }

    /// <summary>
    /// Gets the account number of the parent account.
    /// </summary>
    public string? ParentAccountNumber { get; init; }

    /// <summary>
    /// Gets the name of the parent account.
    /// </summary>
    public string? ParentAccountName { get; init; }

    /// <summary>
    /// Gets a value indicating whether the account is active.
    /// </summary>
    public bool IsActive { get; init; }

    /// <summary>
    /// Gets the date and time when the account was created.
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Gets the date and time when the account was last modified.
    /// </summary>
    public DateTime? ModifiedAt { get; init; }

    /// <summary>
    /// Gets the child accounts in the hierarchy.
    /// </summary>
    public List<ChartOfAccountResponse>? Children { get; init; }
}
