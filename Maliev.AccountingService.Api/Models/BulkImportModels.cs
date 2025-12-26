namespace Maliev.AccountingService.Api.Models;

/// <summary>
/// Result of a bulk import operation
/// </summary>
public class BulkImportResult
{
    public bool Success { get; set; }
    public int TotalRecords { get; set; }
    public int ImportedRecords { get; set; }
    public int SkippedRecords { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public string? Summary { get; set; }
}

/// <summary>
/// CSV record for chart of account import
/// </summary>
public class ChartOfAccountCsvRecord
{
    public string AccountNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Type { get; set; } = string.Empty;
    public string? Category { get; set; }
    public bool? IsActive { get; set; }
}

/// <summary>
/// JSON record for chart of account import
/// </summary>
public class ChartOfAccountJsonRecord
{
    public string AccountNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Type { get; set; } = string.Empty;
    public string? Category { get; set; }
    public bool? IsActive { get; set; }
}

/// <summary>
/// Record for opening balance import
/// </summary>
public class OpeningBalanceRecord
{
    public string AccountNumber { get; set; } = string.Empty;
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public string? Description { get; set; }
}
