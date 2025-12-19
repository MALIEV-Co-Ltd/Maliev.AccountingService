namespace Maliev.AccountingService.Api.DTOs.Responses;

/// <summary>
/// Response DTO for journal entry data
/// </summary>
public class JournalEntryResponse
{
    public Guid Id { get; set; }
    public string EntryNumber { get; set; } = string.Empty;
    public DateTime EntryDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public Guid? PeriodId { get; set; }
    public string? PeriodName { get; set; }
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public Guid? SourceEventId { get; set; }
    public string? Reference { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PostedAt { get; set; }
    public Guid? PostedBy { get; set; }
    public List<JournalEntryLineResponse> Lines { get; set; } = new();
}

/// <summary>
/// Response DTO for journal entry line data
/// </summary>
public class JournalEntryLineResponse
{
    public Guid Id { get; set; }
    public int LineNumber { get; set; }
    public Guid AccountId { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public string? Description { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? SupplierId { get; set; }
    public string? Reference { get; set; }
    public List<TaxComponentResponse>? TaxComponents { get; set; }
}

/// <summary>
/// Response DTO for tax component data
/// </summary>
public class TaxComponentResponse
{
    public Guid Id { get; set; }
    public string TaxType { get; set; } = string.Empty;
    public decimal TaxRate { get; set; }
    public decimal TaxableAmount { get; set; }
    public decimal TaxAmount { get; set; }
}
