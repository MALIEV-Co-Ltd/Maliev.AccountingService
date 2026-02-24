using System.ComponentModel.DataAnnotations;

namespace Maliev.AccountingService.Api.DTOs.Requests;

/// <summary>
/// Request DTO for updating an existing chart of account
/// </summary>
public sealed record UpdateChartOfAccountRequest
{
    /// <summary>
    /// Gets the updated name of the account.
    /// </summary>
    [Required(ErrorMessage = "Account name is required")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Account name must be between 1 and 200 characters")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the updated description.
    /// </summary>
    [StringLength(500, ErrorMessage = "Description must not exceed 500 characters")]
    public string? Description { get; init; }

    /// <summary>
    /// Gets the updated category.
    /// </summary>
    [StringLength(100, ErrorMessage = "Category must not exceed 100 characters")]
    public string? Category { get; init; }

    /// <summary>
    /// Gets the updated parent account ID.
    /// </summary>
    public Guid? ParentAccountId { get; init; }
}
