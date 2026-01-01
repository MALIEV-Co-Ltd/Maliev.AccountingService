namespace Maliev.AccountingService.Api.Events;

/// <summary>
/// Event published by Accounting service when a transaction is posted to the ledger
/// </summary>
public class TransactionPostedEvent
{
    /// <summary>
    /// Gets or sets the event ID.
    /// </summary>
    public Guid EventId { get; set; }

    /// <summary>
    /// Gets or sets the journal entry ID.
    /// </summary>
    public Guid JournalEntryId { get; set; }

    /// <summary>
    /// Gets or sets the unique entry number.
    /// </summary>
    public string EntryNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the entry date.
    /// </summary>
    public DateTime EntryDate { get; set; }

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the total debit amount.
    /// </summary>
    public decimal TotalDebit { get; set; }

    /// <summary>
    /// Gets or sets the total credit amount.
    /// </summary>
    public decimal TotalCredit { get; set; }

    /// <summary>
    /// Gets or sets the IDs of affected accounts.
    /// </summary>
    public List<Guid> AccountsAffected { get; set; } = new();

    /// <summary>
    /// Gets or sets the source event ID.
    /// </summary>
    public Guid? SourceEventId { get; set; }

    /// <summary>
    /// Gets or sets the date and time when posted.
    /// </summary>
    public DateTime PostedAt { get; set; }

    /// <summary>
    /// Gets or sets the ID of the user who posted.
    /// </summary>
    public Guid PostedBy { get; set; }
}
