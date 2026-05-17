using Maliev.AccountingService.Api.Models;
using Maliev.AccountingService.Infrastructure.Data;
using Maliev.AccountingService.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.VisualBasic.FileIO;
using System.Globalization;
using System.Text.Json;

namespace Maliev.AccountingService.Api.Services;

/// <summary>
/// Service for bulk importing chart of accounts and opening balances from CSV/JSON files
/// </summary>
public class BulkImportService : IBulkImportService
{
    private readonly AccountingDbContext _dbContext;
    private readonly ILogger<BulkImportService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="BulkImportService"/> class.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    /// <param name="logger">The logger.</param>
    public BulkImportService(AccountingDbContext dbContext, ILogger<BulkImportService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Imports chart of accounts from a stream.
    /// </summary>
    /// <param name="stream">The file stream.</param>
    /// <param name="fileName">The name of the file.</param>
    /// <param name="dryRun">If true, only validates without importing.</param>
    /// <returns>The result of the import operation.</returns>
    public async Task<BulkImportResult> ImportChartOfAccountsAsync(Stream stream, string fileName, bool dryRun = false)
    {
        var result = new BulkImportResult();

        try
        {
            _logger.LogInformation("Importing chart of accounts from file: {FileName}, DryRun: {DryRun}", fileName, dryRun);

            // Parse file based on extension
            var accounts = Path.GetExtension(fileName).ToLowerInvariant() switch
            {
                ".csv" => ReadAccountsFromCsv(stream),
                ".json" => ReadAccountsFromJson(stream),
                _ => throw new NotSupportedException($"File format {Path.GetExtension(fileName)} not supported. Use .csv or .json")
            };

            result.TotalRecords = accounts.Count;
            _logger.LogInformation("Found {Count} accounts to import", accounts.Count);

            // Validate accounts
            var accountNumbers = new HashSet<string>();

            foreach (var account in accounts)
            {
                if (string.IsNullOrWhiteSpace(account.AccountNumber))
                    result.Errors.Add($"Account missing account number: {account.Name}");

                if (accountNumbers.Contains(account.AccountNumber))
                    result.Errors.Add($"Duplicate account number: {account.AccountNumber}");
                else
                    accountNumbers.Add(account.AccountNumber);

                if (string.IsNullOrWhiteSpace(account.Name))
                    result.Errors.Add($"Account {account.AccountNumber} missing name");
            }

            if (result.Errors.Any())
            {
                result.Success = false;
                result.Summary = $"Validation failed with {result.Errors.Count} error(s)";
                _logger.LogWarning("Validation failed with {ErrorCount} errors", result.Errors.Count);
                return result;
            }

            _logger.LogInformation("Validation passed");

            if (dryRun)
            {
                result.Success = true;
                result.Summary = $"Dry run completed. {accounts.Count} accounts would be imported.";
                return result;
            }

            // Pre-fetch existing account numbers to avoid N+1 queries
            var importAccountNumbers = accounts.Select(a => a.AccountNumber).ToList();
            var existingAccountNumbers = await _dbContext.ChartOfAccounts
                .Where(a => importAccountNumbers.Contains(a.AccountNumber))
                .Select(a => a.AccountNumber)
                .ToListAsync();
            var existingSet = new HashSet<string>(existingAccountNumbers);

            // Import accounts
            foreach (var account in accounts)
            {
                if (existingSet.Contains(account.AccountNumber))
                {
                    result.SkippedRecords++;
                    result.Warnings.Add($"Skipping {account.AccountNumber} (already exists)");
                    continue;
                }

                _dbContext.ChartOfAccounts.Add(account);
                result.ImportedRecords++;
                _logger.LogDebug("Imported {AccountNumber} - {Name}", account.AccountNumber, account.Name);
            }

            await _dbContext.SaveChangesAsync();

            result.Success = true;
            result.Summary = $"Imported {result.ImportedRecords} accounts, skipped {result.SkippedRecords} existing accounts";
            _logger.LogInformation("Import completed: {ImportedCount} imported, {SkippedCount} skipped",
                result.ImportedRecords, result.SkippedRecords);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during chart of accounts import");
            result.Success = false;
            result.Errors.Add($"Import failed: {ex.Message}");
            result.Summary = "Import failed due to error";
            return result;
        }
    }

    /// <summary>
    /// Imports opening balances from a stream.
    /// </summary>
    /// <param name="stream">The file stream.</param>
    /// <param name="fileName">The name of the file.</param>
    /// <param name="dryRun">If true, only validates without importing.</param>
    /// <returns>The result of the import operation.</returns>
    public async Task<BulkImportResult> ImportOpeningBalancesAsync(Stream stream, string fileName, bool dryRun = false)
    {
        var result = new BulkImportResult();

        try
        {
            _logger.LogInformation("Importing opening balances from file: {FileName}, DryRun: {DryRun}", fileName, dryRun);

            // Parse file based on extension
            var balances = Path.GetExtension(fileName).ToLowerInvariant() switch
            {
                ".csv" => ReadBalancesFromCsv(stream),
                ".json" => ReadBalancesFromJson(stream),
                _ => throw new NotSupportedException($"File format {Path.GetExtension(fileName)} not supported. Use .csv or .json")
            };

            result.TotalRecords = balances.Count;
            _logger.LogInformation("Found {Count} opening balances to import", balances.Count);

            // Pre-fetch all referenced accounts to avoid N+1 queries during validation and import
            var accountNumbers = balances.Select(b => b.AccountNumber).Distinct().ToList();
            var existingAccounts = await _dbContext.ChartOfAccounts
                .Where(a => accountNumbers.Contains(a.AccountNumber))
                .ToDictionaryAsync(a => a.AccountNumber);

            // Validate balances
            decimal totalDebits = 0;
            decimal totalCredits = 0;

            foreach (var balance in balances)
            {
                if (!existingAccounts.TryGetValue(balance.AccountNumber, out var account))
                {
                    result.Errors.Add($"Account not found: {balance.AccountNumber}");
                    continue;
                }

                totalDebits += balance.DebitAmount;
                totalCredits += balance.CreditAmount;
            }

            if (Math.Abs(totalDebits - totalCredits) > 0.01m)
            {
                result.Errors.Add($"Opening balances not balanced: Debits={totalDebits:F2}, Credits={totalCredits:F2}, Difference={totalDebits - totalCredits:F2}");
            }

            if (result.Errors.Any())
            {
                result.Success = false;
                result.Summary = $"Validation failed with {result.Errors.Count} error(s)";
                _logger.LogWarning("Validation failed with {ErrorCount} errors", result.Errors.Count);
                return result;
            }

            _logger.LogInformation("Validation passed! Balanced: Debits={Debits:F2}, Credits={Credits:F2}", totalDebits, totalCredits);

            if (dryRun)
            {
                result.Success = true;
                result.Summary = $"Dry run completed. {balances.Count} opening balances validated. Balanced: DR={totalDebits:F2}, CR={totalCredits:F2}";
                return result;
            }

            // Implementation of actual import: Create a manual journal entry for opening balances
            var openingDate = new DateTime(DateTime.UtcNow.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc); // Default to start of year
            var periodService = _dbContext.GetService<IPeriodService>();
            var period = await periodService.GetOrCreatePeriodAsync(openingDate);

            var journalEntry = new JournalEntry
            {
                Id = Guid.NewGuid(),
                PeriodId = period.Id,
                EntryNumber = await periodService.GenerateEntryNumberAsync(period.Id),
                EntryDate = openingDate,
                Description = "Opening Balances Import",
                Status = EntryStatus.Posted,
                SourceSystem = "System",
                CreatedBy = Guid.Empty,
                PostedAt = DateTime.UtcNow,
                PostedBy = Guid.Empty,
                CurrencyCode = "THB",
                ExchangeRateToBase = 1m,
                TotalDebit = totalDebits,
                TotalCredit = totalCredits,
                TransactionTotalDebit = totalDebits,
                TransactionTotalCredit = totalCredits
            };

            int seq = 1;
            foreach (var balance in balances)
            {
                var account = existingAccounts[balance.AccountNumber];

                journalEntry.Lines.Add(new JournalEntryLine
                {
                    Id = Guid.NewGuid(),
                    JournalEntryId = journalEntry.Id,
                    AccountId = account.Id,
                    LineSequence = seq++,
                    Description = "Opening balance",
                    DebitAmount = balance.DebitAmount,
                    CreditAmount = balance.CreditAmount,
                    TransactionDebitAmount = balance.DebitAmount,
                    TransactionCreditAmount = balance.CreditAmount
                });
            }

            _dbContext.JournalEntries.Add(journalEntry);
            await _dbContext.SaveChangesAsync();

            result.Success = true;
            result.ImportedRecords = balances.Count;
            result.Summary = $"Successfully imported {balances.Count} opening balances into journal entry {journalEntry.EntryNumber}";

            _logger.LogInformation("Opening balances imported successfully: {EntryNumber}", journalEntry.EntryNumber);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during opening balances import");
            result.Success = false;
            result.Errors.Add($"Import failed: {ex.Message}");
            result.Summary = "Import failed due to error";
            return result;
        }
    }

    private List<ChartOfAccount> ReadAccountsFromCsv(Stream stream)
    {
        var records = new List<ChartOfAccount>();
        using var parser = new TextFieldParser(stream);
        parser.TextFieldType = FieldType.Delimited;
        parser.SetDelimiters(",");
        parser.HasFieldsEnclosedInQuotes = true;

        if (parser.EndOfData) return new List<ChartOfAccount>();

        var headers = parser.ReadFields();
        if (headers == null) return new List<ChartOfAccount>();

        var headerMap = headers.Select((h, i) => new { Name = h.Trim(), Index = i })
                               .ToDictionary(h => h.Name, h => h.Index, StringComparer.OrdinalIgnoreCase);

        while (!parser.EndOfData)
        {
            var fields = parser.ReadFields();
            if (fields == null) continue;

            string GetField(string name, bool required = true)
            {
                if (headerMap.TryGetValue(name, out var index) && index < fields.Length)
                {
                    return fields[index];
                }
                if (required)
                {
                    throw new InvalidDataException($"Missing required field '{name}' in CSV file");
                }
                return string.Empty;
            }

            records.Add(new ChartOfAccount
            {
                Id = Guid.NewGuid(),
                AccountNumber = GetField(nameof(ChartOfAccountCsvRecord.AccountNumber)),
                Name = GetField(nameof(ChartOfAccountCsvRecord.Name)),
                Description = GetField(nameof(ChartOfAccountCsvRecord.Description), false),
                Type = Enum.Parse<AccountType>(GetField(nameof(ChartOfAccountCsvRecord.Type)), true),
                Category = GetField(nameof(ChartOfAccountCsvRecord.Category), false),
                IsActive = bool.TryParse(GetField(nameof(ChartOfAccountCsvRecord.IsActive), false), out var isActive) ? isActive : true,
                CreatedAt = DateTime.UtcNow
            });
        }

        return records;
    }

    private List<ChartOfAccount> ReadAccountsFromJson(Stream stream)
    {
        using var reader = new StreamReader(stream);
        var json = reader.ReadToEnd();

        var records = JsonSerializer.Deserialize<List<ChartOfAccountJsonRecord>>(json)
            ?? throw new InvalidOperationException("Failed to deserialize JSON");

        return records.Select(r => new ChartOfAccount
        {
            Id = Guid.NewGuid(),
            AccountNumber = r.AccountNumber,
            Name = r.Name,
            Description = r.Description,
            Type = Enum.Parse<AccountType>(r.Type, true),
            Category = r.Category,
            IsActive = r.IsActive ?? true,
            CreatedAt = DateTime.UtcNow
        }).ToList();
    }

    private List<OpeningBalanceRecord> ReadBalancesFromCsv(Stream stream)
    {
        var records = new List<OpeningBalanceRecord>();
        using var parser = new TextFieldParser(stream);
        parser.TextFieldType = FieldType.Delimited;
        parser.SetDelimiters(",");
        parser.HasFieldsEnclosedInQuotes = true;

        if (parser.EndOfData) return new List<OpeningBalanceRecord>();

        var headers = parser.ReadFields();
        if (headers == null) return new List<OpeningBalanceRecord>();

        var headerMap = headers.Select((h, i) => new { Name = h.Trim(), Index = i })
                               .ToDictionary(h => h.Name, h => h.Index, StringComparer.OrdinalIgnoreCase);

        while (!parser.EndOfData)
        {
            var fields = parser.ReadFields();
            if (fields == null) continue;

            string GetField(string name, bool required = true)
            {
                if (headerMap.TryGetValue(name, out var index) && index < fields.Length)
                {
                    return fields[index];
                }
                if (required)
                {
                    throw new InvalidDataException($"Missing required field '{name}' in CSV file");
                }
                return string.Empty;
            }

            records.Add(new OpeningBalanceRecord
            {
                AccountNumber = GetField(nameof(OpeningBalanceRecord.AccountNumber)),
                DebitAmount = decimal.TryParse(GetField(nameof(OpeningBalanceRecord.DebitAmount)), NumberStyles.Any, CultureInfo.InvariantCulture, out var debit) ? debit : 0,
                CreditAmount = decimal.TryParse(GetField(nameof(OpeningBalanceRecord.CreditAmount)), NumberStyles.Any, CultureInfo.InvariantCulture, out var credit) ? credit : 0,
                Description = GetField(nameof(OpeningBalanceRecord.Description), false)
            });
        }

        return records;
    }

    private List<OpeningBalanceRecord> ReadBalancesFromJson(Stream stream)
    {
        using var reader = new StreamReader(stream);
        var json = reader.ReadToEnd();

        return JsonSerializer.Deserialize<List<OpeningBalanceRecord>>(json)
            ?? throw new InvalidOperationException("Failed to deserialize JSON");
    }
}
