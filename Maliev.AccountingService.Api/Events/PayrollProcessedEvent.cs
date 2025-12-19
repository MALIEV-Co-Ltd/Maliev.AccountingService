namespace Maliev.AccountingService.Api.Events;

/// <summary>
/// Event published by Payroll service when payroll is processed
/// </summary>
public class PayrollProcessedEvent
{
    public string EventId { get; set; } = string.Empty;
    public Guid PayrollId { get; set; }
    public string PayrollNumber { get; set; } = string.Empty;
    public string? PayrollPeriod { get; set; }
    public DateTime? ProcessedDate { get; set; }
    public DateTime PayPeriodStart { get; set; }
    public DateTime PayPeriodEnd { get; set; }
    public DateTime PaymentDate { get; set; }
    public decimal GrossPay { get; set; }
    public decimal? EmployeeTax { get; set; }
    public decimal? SocialSecurity { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal NetPay { get; set; }
    public List<PayrollDeduction> Deductions { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class PayrollDeduction
{
    public string DeductionType { get; set; } = string.Empty; // Tax, Insurance, Pension, etc.
    public decimal Amount { get; set; }
}
