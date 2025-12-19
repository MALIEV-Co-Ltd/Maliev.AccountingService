using System.ComponentModel.DataAnnotations;

namespace Maliev.AccountingService.Data.Models;

public class FiscalYear
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [Required]
    public PeriodStructure PeriodStructure { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<FinancialPeriod> FinancialPeriods { get; set; } = new List<FinancialPeriod>();
}

public enum PeriodStructure
{
    Monthly,
    Quarterly
}
