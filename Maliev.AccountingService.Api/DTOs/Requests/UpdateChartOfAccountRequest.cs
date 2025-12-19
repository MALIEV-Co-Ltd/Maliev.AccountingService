using System.ComponentModel.DataAnnotations;

namespace Maliev.AccountingService.Api.DTOs.Requests;

/// <summary>
/// Request DTO for updating an existing chart of account
/// </summary>
public sealed record UpdateChartOfAccountRequest
{
    [Required(ErrorMessage = "Account name is required")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Account name must be between 1 and 200 characters")]
    public string Name { get; init; } = string.Empty;

    [StringLength(500, ErrorMessage = "Description must not exceed 500 characters")]
    public string? Description { get; init; }

    [StringLength(100, ErrorMessage = "Category must not exceed 100 characters")]
    public string? Category { get; init; }

    public Guid? ParentAccountId { get; init; }
}
