namespace Maliev.AccountingService.Api.Events;

/// <summary>
/// Event published by Accounting service when a transaction is posted to the ledger
/// </summary>
public class TransactionPostedEvent
{
    public Guid EventId { get; set; }
    public Guid JournalEntryId { get; set; }
    public string EntryNumber { get; set; } = string.Empty;
    public DateTime EntryDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public List<Guid> AccountsAffected { get; set; } = new();
    public Guid? SourceEventId { get; set; }
    public DateTime PostedAt { get; set; }
    public Guid PostedBy { get; set; }
}
