using System.ComponentModel.DataAnnotations;

namespace Maliev.AccountingService.Infrastructure.Models;

public class FinancialPeriod
{
    public Guid Id { get; set; }

    [Required]
    public Guid FiscalYearId { get; set; }

    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [Required]
    public PeriodStatus Status { get; set; } = PeriodStatus.Open;

    public DateTime? ClosedAt { get; set; }

    public Guid? ClosedBy { get; set; }

    // Navigation properties
    public FiscalYear FiscalYear { get; set; } = null!;
    public ICollection<JournalEntry> JournalEntries { get; set; } = new List<JournalEntry>();
    public ICollection<ReconciliationReport> ReconciliationReports { get; set; } = new List<ReconciliationReport>();
}

public enum PeriodStatus
{
    Open,
    Closed,
    Locked
}
