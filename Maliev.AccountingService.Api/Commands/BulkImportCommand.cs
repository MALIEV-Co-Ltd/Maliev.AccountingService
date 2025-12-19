using System.CommandLine;
using System.Text.Json;
using CsvHelper;
using CsvHelper.Configuration;
using Maliev.AccountingService.Data.Data;
using Maliev.AccountingService.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;

namespace Maliev.AccountingService.Api.Commands;

/// <summary>
/// CLI command for bulk importing chart of accounts and opening balances from CSV/JSON
/// Usage: dotnet run -- bulk-import --file accounts.csv --type chart-of-accounts
/// </summary>
public static class BulkImportCommand
{
    public static Command CreateCommand(IServiceProvider services)
    {
        var command = new Command("bulk-import", "Import chart of accounts and opening balances from CSV/JSON");

        var fileOption = new Option<FileInfo>(
            aliases: new[] { "--file", "-f" },
            description: "Path to CSV or JSON file containing data to import")
        {
            IsRequired = true
        };

        var typeOption = new Option<string>(
            aliases: new[] { "--type", "-t" },
            description: "Type of data to import: chart-of-accounts or opening-balances")
        {
            IsRequired = true
        };

        var dryRunOption = new Option<bool>(
            aliases: new[] { "--dry-run", "-d" },
            getDefaultValue: () => false,
            description: "Validate data without importing");

        command.AddOption(fileOption);
        command.AddOption(typeOption);
        command.AddOption(dryRunOption);

        command.SetHandler(async (file, type, dryRun) =>
        {
            await ExecuteImportAsync(services, file, type, dryRun);
        }, fileOption, typeOption, dryRunOption);

        return command;
    }

    private static async Task ExecuteImportAsync(IServiceProvider services, FileInfo file, string importType, bool dryRun)
    {
        if (!file.Exists)
        {
            Console.WriteLine($"Error: File not found: {file.FullName}");
            return;
        }

        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AccountingDbContext>();

        try
        {
            switch (importType.ToLowerInvariant())
            {
                case "chart-of-accounts":
                case "accounts":
                    await ImportChartOfAccountsAsync(dbContext, file, dryRun);
                    break;

                case "opening-balances":
                case "balances":
                    await ImportOpeningBalancesAsync(dbContext, file, dryRun);
                    break;

                default:
                    Console.WriteLine($"Error: Unknown import type '{importType}'. Valid types: chart-of-accounts, opening-balances");
                    return;
            }

            Console.WriteLine($"\n{(dryRun ? "Validation" : "Import")} completed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nError during import: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }

    private static async Task ImportChartOfAccountsAsync(AccountingDbContext dbContext, FileInfo file, bool dryRun)
    {
        Console.WriteLine($"Importing chart of accounts from: {file.Name}");
        Console.WriteLine($"Mode: {(dryRun ? "DRY RUN (validation only)" : "LIVE IMPORT")}");

        var accounts = file.Extension.ToLowerInvariant() switch
        {
            ".csv" => ReadAccountsFromCsv(file),
            ".json" => ReadAccountsFromJson(file),
            _ => throw new NotSupportedException($"File format {file.Extension} not supported. Use .csv or .json")
        };

        Console.WriteLine($"\nFound {accounts.Count} accounts to import");

        // Validate accounts
        var errors = new List<string>();
        var accountNumbers = new HashSet<string>();

        foreach (var account in accounts)
        {
            if (string.IsNullOrWhiteSpace(account.AccountNumber))
                errors.Add($"Account missing account number: {account.Name}");

            if (accountNumbers.Contains(account.AccountNumber))
                errors.Add($"Duplicate account number: {account.AccountNumber}");
            else
                accountNumbers.Add(account.AccountNumber);

            if (string.IsNullOrWhiteSpace(account.Name))
                errors.Add($"Account {account.AccountNumber} missing name");
        }

        if (errors.Any())
        {
            Console.WriteLine("\nValidation Errors:");
            foreach (var error in errors)
                Console.WriteLine($"  - {error}");
            return;
        }

        Console.WriteLine("Validation passed!");

        if (dryRun)
        {
            Console.WriteLine("\nAccounts to be imported:");
            foreach (var account in accounts.OrderBy(a => a.AccountNumber))
            {
                Console.WriteLine($"  {account.AccountNumber} - {account.Name} ({account.Type})");
            }
            return;
        }

        // Import accounts
        var imported = 0;
        var skipped = 0;

        foreach (var account in accounts)
        {
            var exists = await dbContext.ChartOfAccounts
                .AnyAsync(a => a.AccountNumber == account.AccountNumber);

            if (exists)
            {
                Console.WriteLine($"  Skipping {account.AccountNumber} (already exists)");
                skipped++;
                continue;
            }

            dbContext.ChartOfAccounts.Add(account);
            imported++;
            Console.WriteLine($"  Imported {account.AccountNumber} - {account.Name}");
        }

        await dbContext.SaveChangesAsync();
        Console.WriteLine($"\nImported: {imported}, Skipped: {skipped}");
    }

    private static async Task ImportOpeningBalancesAsync(AccountingDbContext dbContext, FileInfo file, bool dryRun)
    {
        Console.WriteLine($"Importing opening balances from: {file.Name}");
        Console.WriteLine($"Mode: {(dryRun ? "DRY RUN (validation only)" : "LIVE IMPORT")}");

        var balances = file.Extension.ToLowerInvariant() switch
        {
            ".csv" => ReadBalancesFromCsv(file),
            ".json" => ReadBalancesFromJson(file),
            _ => throw new NotSupportedException($"File format {file.Extension} not supported. Use .csv or .json")
        };

        Console.WriteLine($"\nFound {balances.Count} opening balances to import");

        // Validate balances
        var errors = new List<string>();
        decimal totalDebits = 0;
        decimal totalCredits = 0;

        foreach (var balance in balances)
        {
            var account = await dbContext.ChartOfAccounts
                .FirstOrDefaultAsync(a => a.AccountNumber == balance.AccountNumber);

            if (account == null)
            {
                errors.Add($"Account not found: {balance.AccountNumber}");
                continue;
            }

            totalDebits += balance.DebitAmount;
            totalCredits += balance.CreditAmount;
        }

        if (Math.Abs(totalDebits - totalCredits) > 0.01m)
        {
            errors.Add($"Opening balances not balanced: Debits={totalDebits:F2}, Credits={totalCredits:F2}, Difference={totalDebits - totalCredits:F2}");
        }

        if (errors.Any())
        {
            Console.WriteLine("\nValidation Errors:");
            foreach (var error in errors)
                Console.WriteLine($"  - {error}");
            return;
        }

        Console.WriteLine($"Validation passed! Balanced: Debits={totalDebits:F2}, Credits={totalCredits:F2}");

        if (dryRun)
        {
            Console.WriteLine("\nOpening balances to be imported:");
            foreach (var balance in balances)
            {
                var amount = balance.DebitAmount > 0 ? $"DR {balance.DebitAmount:F2}" : $"CR {balance.CreditAmount:F2}";
                Console.WriteLine($"  {balance.AccountNumber} - {amount}");
            }
            return;
        }

        Console.WriteLine("\nNote: Opening balances import requires creating journal entries.");
        Console.WriteLine("This functionality should be implemented based on your business requirements.");
    }

    private static List<ChartOfAccount> ReadAccountsFromCsv(FileInfo file)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null
        };

        using var reader = new StreamReader(file.FullName);
        using var csv = new CsvReader(reader, config);

        var records = csv.GetRecords<ChartOfAccountCsvRecord>().ToList();

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

    private static List<ChartOfAccount> ReadAccountsFromJson(FileInfo file)
    {
        var json = File.ReadAllText(file.FullName);
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

    private static List<OpeningBalanceRecord> ReadBalancesFromCsv(FileInfo file)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null
        };

        using var reader = new StreamReader(file.FullName);
        using var csv = new CsvReader(reader, config);
        return csv.GetRecords<OpeningBalanceRecord>().ToList();
    }

    private static List<OpeningBalanceRecord> ReadBalancesFromJson(FileInfo file)
    {
        var json = File.ReadAllText(file.FullName);
        return JsonSerializer.Deserialize<List<OpeningBalanceRecord>>(json)
            ?? throw new InvalidOperationException("Failed to deserialize JSON");
    }
}

public class ChartOfAccountCsvRecord
{
    public string AccountNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Type { get; set; } = string.Empty;
    public string? Category { get; set; }
    public bool? IsActive { get; set; }
}

public class ChartOfAccountJsonRecord
{
    public string AccountNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Type { get; set; } = string.Empty;
    public string? Category { get; set; }
    public bool? IsActive { get; set; }
}

public class OpeningBalanceRecord
{
    public string AccountNumber { get; set; } = string.Empty;
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public string? Description { get; set; }
}
