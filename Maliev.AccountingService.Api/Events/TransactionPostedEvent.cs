namespace Maliev.AccountingService.Api.Events;

/// <summary>
/// Event published when a financial transaction is posted to the ledger
/// </summary>
public record TransactionPostedEvent
{
    /// <summary>The ID of the journal entry.</summary>
    public Guid JournalEntryId { get; init; }
    /// <summary>The human-readable entry number.</summary>
    public string EntryNumber { get; init; } = string.Empty;
    /// <summary>The date the entry was recorded.</summary>
    public DateTime EntryDate { get; init; }
    /// <summary>A description of the transaction.</summary>
    public string Description { get; init; } = string.Empty;
    /// <summary>The total debit amount.</summary>
    public decimal TotalAmount { get; init; }
    /// <summary>The source system that originated the event.</summary>
    public string SourceSystem { get; init; } = string.Empty;
    /// <summary>The timestamp when the entry was posted.</summary>
    public DateTime PostedAt { get; init; }
}