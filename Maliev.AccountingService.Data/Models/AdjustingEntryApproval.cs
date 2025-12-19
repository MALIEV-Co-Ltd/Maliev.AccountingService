using System.ComponentModel.DataAnnotations;

namespace Maliev.AccountingService.Data.Models;

public class AdjustingEntryApproval
{
    public Guid Id { get; set; }

    [Required]
    public Guid JournalEntryId { get; set; }

    [Required]
    public Guid RequestedBy { get; set; }

    [Required]
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

    [Required]
    [StringLength(1000)]
    public string Reason { get; set; } = string.Empty;

    public Guid? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    [StringLength(50)]
    public string Status { get; set; } = "Pending";

    [StringLength(1000)]
    public string? ApprovalComments { get; set; }

    // Navigation properties
    public JournalEntry JournalEntry { get; set; } = null!;
}
