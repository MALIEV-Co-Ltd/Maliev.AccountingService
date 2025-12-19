namespace Maliev.AccountingService.Api.DTOs.Responses;

/// <summary>
/// Response DTO for chart of account information
/// </summary>
public sealed record ChartOfAccountResponse
{
    public Guid Id { get; init; }
    public string AccountNumber { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Type { get; init; } = string.Empty;
    public string? Category { get; init; }
    public Guid? ParentAccountId { get; init; }
    public string? ParentAccountNumber { get; init; }
    public string? ParentAccountName { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ModifiedAt { get; init; }
    public List<ChartOfAccountResponse>? Children { get; init; }
}
