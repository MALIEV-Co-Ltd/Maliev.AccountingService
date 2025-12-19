using System.ComponentModel.DataAnnotations;

namespace Maliev.AccountingService.Api.DTOs.Requests;

/// <summary>
/// Request DTO for creating a new chart of account
/// </summary>
public sealed record CreateChartOfAccountRequest
{
    [Required(ErrorMessage = "Account number is required")]
    [StringLength(50, MinimumLength = 4, ErrorMessage = "Account number must be between 4 and 50 characters")]
    public string AccountNumber { get; init; } = string.Empty;

    [Required(ErrorMessage = "Account name is required")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Account name must be between 1 and 200 characters")]
    public string Name { get; init; } = string.Empty;

    [StringLength(500, ErrorMessage = "Description must not exceed 500 characters")]
    public string? Description { get; init; }

    [Required(ErrorMessage = "Account type is required")]
    [RegularExpression("^(Asset|Liability|Equity|Revenue|Expense)$",
        ErrorMessage = "Account type must be one of: Asset, Liability, Equity, Revenue, Expense")]
    public string Type { get; init; } = string.Empty;

    [StringLength(100, ErrorMessage = "Category must not exceed 100 characters")]
    public string? Category { get; init; }

    public Guid? ParentAccountId { get; init; }

    public bool IsActive { get; init; } = true;
}
