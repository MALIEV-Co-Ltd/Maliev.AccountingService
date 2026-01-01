namespace Maliev.AccountingService.Api.Events;

/// <summary>
/// Event published by Payroll service when payroll is processed
/// </summary>
public class PayrollProcessedEvent
{
    /// <summary>
    /// Gets or sets the event ID.
    /// </summary>
    public string EventId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the payroll ID.
    /// </summary>
    public Guid PayrollId { get; set; }

    /// <summary>
    /// Gets or sets the payroll number.
    /// </summary>
    public string PayrollNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the payroll period name.
    /// </summary>
    public string? PayrollPeriod { get; set; }

    /// <summary>
    /// Gets or sets the date when payroll was processed.
    /// </summary>
    public DateTime? ProcessedDate { get; set; }

    /// <summary>
    /// Gets or sets the start date of the pay period.
    /// </summary>
    public DateTime PayPeriodStart { get; set; }

    /// <summary>
    /// Gets or sets the end date of the pay period.
    /// </summary>
    public DateTime PayPeriodEnd { get; set; }

    /// <summary>
    /// Gets or sets the payment date.
    /// </summary>
    public DateTime PaymentDate { get; set; }

    /// <summary>
    /// Gets or sets the total gross pay.
    /// </summary>
    public decimal GrossPay { get; set; }

    /// <summary>
    /// Gets or sets the employee tax amount.
    /// </summary>
    public decimal? EmployeeTax { get; set; }

    /// <summary>
    /// Gets or sets the social security contribution.
    /// </summary>
    public decimal? SocialSecurity { get; set; }

    /// <summary>
    /// Gets or sets the total deductions.
    /// </summary>
    public decimal TotalDeductions { get; set; }

    /// <summary>
    /// Gets or sets the net pay.
    /// </summary>
    public decimal NetPay { get; set; }

    /// <summary>
    /// Gets or sets the breakdown of deductions.
    /// </summary>
    public List<PayrollDeduction> Deductions { get; set; } = new();

    /// <summary>
    /// Gets or sets the creation date.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Represents a deduction in a payroll processed event
/// </summary>
public class PayrollDeduction
{
    /// <summary>
    /// Gets or sets the type of deduction (e.g., Tax, Insurance, Pension).
    /// </summary>
    public string DeductionType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the deduction amount.
    /// </summary>
    public decimal Amount { get; set; }
}
