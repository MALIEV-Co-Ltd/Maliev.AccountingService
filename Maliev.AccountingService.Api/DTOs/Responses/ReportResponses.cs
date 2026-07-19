namespace Maliev.AccountingService.Api.DTOs.Responses;

/// <summary>Represents a Trial Balance report</summary>
public class TrialBalanceResponse
{
    /// <summary>Name of the period or custom range</summary>
    public string PeriodName { get; set; } = string.Empty;
    /// <summary>When the report was generated</summary>
    public DateTime GeneratedAt { get; set; }
    /// <summary>Lines of the report</summary>
    public List<TrialBalanceLine> Items { get; set; } = new();
    /// <summary>Sum of all debits</summary>
    public decimal TotalDebit { get; set; }
    /// <summary>Sum of all credits</summary>
    public decimal TotalCredit { get; set; }
}

/// <summary>Represents a line in the Trial Balance report</summary>
public class TrialBalanceLine
{
    /// <summary>Unique account number</summary>
    public string AccountNumber { get; set; } = string.Empty;
    /// <summary>Account name</summary>
    public string AccountName { get; set; } = string.Empty;
    /// <summary>Debit balance if any</summary>
    public decimal DebitBalance { get; set; }
    /// <summary>Credit balance if any</summary>
    public decimal CreditBalance { get; set; }
}

/// <summary>Represents a Balance Sheet report</summary>
public class BalanceSheetResponse
{
    /// <summary>Date as of which the report is generated</summary>
    public DateTime AsOfDate { get; set; }
    /// <summary>When the report was generated</summary>
    public DateTime GeneratedAt { get; set; }
    /// <summary>Asset sections</summary>
    public List<BalanceSheetSection> Assets { get; set; } = new();
    /// <summary>Liability sections</summary>
    public List<BalanceSheetSection> Liabilities { get; set; } = new();
    /// <summary>Equity sections</summary>
    public List<BalanceSheetSection> Equity { get; set; } = new();
    /// <summary>Total asset value</summary>
    public decimal TotalAssets { get; set; }
    /// <summary>Total liability value</summary>
    public decimal TotalLiabilities { get; set; }
    /// <summary>Total equity value</summary>
    public decimal TotalEquity { get; set; }
}

/// <summary>Represents a section in the Balance Sheet</summary>
public class BalanceSheetSection
{
    /// <summary>Category name</summary>
    public string Category { get; set; } = string.Empty;
    /// <summary>Items in this section</summary>
    public List<BalanceSheetItem> Items { get; set; } = new();
    /// <summary>Section subtotal</summary>
    public decimal Subtotal { get; set; }
}

/// <summary>Represents an item in a Balance Sheet section</summary>
public class BalanceSheetItem
{
    /// <summary>Account number</summary>
    public string AccountNumber { get; set; } = string.Empty;
    /// <summary>Account name</summary>
    public string AccountName { get; set; } = string.Empty;
    /// <summary>Current balance</summary>
    public decimal Balance { get; set; }
}

/// <summary>Represents an Income Statement report</summary>
public class IncomeStatementResponse
{
    /// <summary>Reporting period start date</summary>
    public DateTime StartDate { get; set; }
    /// <summary>Reporting period end date</summary>
    public DateTime EndDate { get; set; }
    /// <summary>When the report was generated</summary>
    public DateTime GeneratedAt { get; set; }
    /// <summary>Revenue sections</summary>
    public List<IncomeStatementSection> Revenues { get; set; } = new();
    /// <summary>Expense sections</summary>
    public List<IncomeStatementSection> Expenses { get; set; } = new();
    /// <summary>Total revenue</summary>
    public decimal TotalRevenue { get; set; }
    /// <summary>Total expense</summary>
    public decimal TotalExpense { get; set; }
    /// <summary>Net income (TotalRevenue - TotalExpense)</summary>
    public decimal NetIncome => TotalRevenue - TotalExpense;
}

/// <summary>Represents a section in the Income Statement</summary>
public class IncomeStatementSection
{
    /// <summary>Category name</summary>
    public string Category { get; set; } = string.Empty;
    /// <summary>Items in this section</summary>
    public List<IncomeStatementItem> Items { get; set; } = new();
    /// <summary>Section subtotal</summary>
    public decimal Subtotal { get; set; }
}

/// <summary>Represents an item in an Income Statement section</summary>
public class IncomeStatementItem
{
    /// <summary>Account number</summary>
    public string AccountNumber { get; set; } = string.Empty;
    /// <summary>Account name</summary>
    public string AccountName { get; set; } = string.Empty;
    /// <summary>Amount for the period</summary>
    public decimal Amount { get; set; }
}
