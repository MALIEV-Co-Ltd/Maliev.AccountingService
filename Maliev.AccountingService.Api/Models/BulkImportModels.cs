namespace Maliev.AccountingService.Api.Models;

/// <summary>
/// Result of a bulk import operation
/// </summary>
public class BulkImportResult
{
    /// <summary>
    /// Gets or sets a value indicating whether the import was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the total number of records processed.
    /// </summary>
    public int TotalRecords { get; set; }

    /// <summary>
    /// Gets or sets the number of records successfully imported.
    /// </summary>
    public int ImportedRecords { get; set; }

    /// <summary>
    /// Gets or sets the number of records skipped.
    /// </summary>
    public int SkippedRecords { get; set; }

    /// <summary>
    /// Gets or sets the list of errors encountered.
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of warnings encountered.
    /// </summary>
    public List<string> Warnings { get; set; } = new();

    /// <summary>
    /// Gets or sets the summary of the operation.
    /// </summary>
    public string? Summary { get; set; }
}

/// <summary>
/// CSV record for chart of account import
/// </summary>
public class ChartOfAccountCsvRecord
{
    /// <summary>
    /// Gets or sets the account number.
    /// </summary>
    public string AccountNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the account name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the account description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the account type.
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the account category.
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the account is active.
    /// </summary>
    public bool? IsActive { get; set; }
}

/// <summary>
/// JSON record for chart of account import
/// </summary>
public class ChartOfAccountJsonRecord
{
    /// <summary>
    /// Gets or sets the account number.
    /// </summary>
    public string AccountNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the account name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the account description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the account type.
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the account category.
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the account is active.
    /// </summary>
    public bool? IsActive { get; set; }
}

/// <summary>
/// Record for opening balance import
/// </summary>
public class OpeningBalanceRecord
{
    /// <summary>
    /// Gets or sets the account number.
    /// </summary>
    public string AccountNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the debit amount.
    /// </summary>
    public decimal DebitAmount { get; set; }

    /// <summary>
    /// Gets or sets the credit amount.
    /// </summary>
    public decimal CreditAmount { get; set; }

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string? Description { get; set; }
}
