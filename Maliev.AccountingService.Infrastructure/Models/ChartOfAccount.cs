using System.ComponentModel.DataAnnotations;

namespace Maliev.AccountingService.Infrastructure.Models;

public class ChartOfAccount
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(50)]
    public string AccountNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    public AccountType Type { get; set; }

    [StringLength(100)]
    public string? Category { get; set; }

    public Guid? ParentAccountId { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ModifiedAt { get; set; }

    // Navigation properties
    public ChartOfAccount? ParentAccount { get; set; }
    public ICollection<ChartOfAccount> ChildAccounts { get; set; } = new List<ChartOfAccount>();
    public ICollection<JournalEntryLine> JournalEntryLines { get; set; } = new List<JournalEntryLine>();
}

public enum AccountType
{
    Asset,
    Liability,
    Equity,
    Revenue,
    Expense
}
