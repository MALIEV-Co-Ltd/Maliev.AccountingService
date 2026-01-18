using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Maliev.AccountingService.Api.Models;
using Maliev.AccountingService.Data.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Maliev.AccountingService.Tests.Integration;

public class BulkImportTests : BaseIntegrationTest
{
    public BulkImportTests(IntegrationTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task ImportChartOfAccounts_Csv_ShouldSucceed()
    {
        await CleanDatabaseAsync();

        // Arrange
        var csvContent = new StringBuilder();
        csvContent.AppendLine("AccountNumber,Name,Type,Category,IsActive,Description");
        csvContent.AppendLine("1000,Cash,Asset,Current Asset,True,Main cash account");
        csvContent.AppendLine("2000,Accounts Payable,Liability,Current Liability,True,Unpaid bills");

        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(csvContent.ToString()));
        var formData = new MultipartFormDataContent();
        formData.Add(fileContent, "file", "accounts.csv");

        // Act
        var response = await Client.PostAsync("/accounting/v1/bulk-import/chart-of-accounts", formData);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<BulkImportResult>();
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(2, result.ImportedRecords);

        var dbContext = Factory.GetDbContext();
        var accounts = await dbContext.ChartOfAccounts.ToListAsync();
        Assert.Equal(2, accounts.Count);
        Assert.Contains(accounts, a => a.AccountNumber == "1000");
        Assert.Contains(accounts, a => a.AccountNumber == "2000");
    }

    [Fact]
    public async Task ImportChartOfAccounts_Json_ShouldSucceed()
    {
        await CleanDatabaseAsync();

        // Arrange
        var jsonContent = JsonSerializer.Serialize(new[]
        {
            new { AccountNumber = "3000", Name = "Revenue", Type = "Revenue", Category = "Operating", IsActive = true },
            new { AccountNumber = "4000", Name = "Salaries", Type = "Expense", Category = "Operating", IsActive = true }
        });

        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(jsonContent));
        var formData = new MultipartFormDataContent();
        formData.Add(fileContent, "file", "accounts.json");

        // Act
        var response = await Client.PostAsync("/accounting/v1/bulk-import/chart-of-accounts", formData);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<BulkImportResult>();
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(2, result.ImportedRecords);
    }

    [Fact]
    public async Task ImportChartOfAccounts_DryRun_ShouldNotSaveToDb()
    {
        await CleanDatabaseAsync();

        // Arrange
        var csvContent = "AccountNumber,Name,Type,Category,IsActive\n5000,Test Account,Asset,Category,True";
        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(csvContent));
        var formData = new MultipartFormDataContent();
        formData.Add(fileContent, "file", "accounts.csv");

        // Act
        var response = await Client.PostAsync("/accounting/v1/bulk-import/chart-of-accounts?dryRun=true", formData);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<BulkImportResult>();
        Assert.NotNull(result);
        Assert.True(result.Success);

        var dbContext = Factory.GetDbContext();
        var accountExists = await dbContext.ChartOfAccounts.AnyAsync(a => a.AccountNumber == "5000");
        Assert.False(accountExists);
    }

    [Fact]
    public async Task ImportChartOfAccounts_InvalidFormat_ShouldReturnBadRequest()
    {
        // Arrange
        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes("invalid content"));
        var formData = new MultipartFormDataContent();
        formData.Add(fileContent, "file", "accounts.txt"); // .txt not supported

        // Act
        var response = await Client.PostAsync("/accounting/v1/bulk-import/chart-of-accounts", formData);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ImportOpeningBalances_Csv_ShouldSucceed_WhenBalanced()
    {
        await CleanDatabaseAsync();

        // Arrange - Seed accounts first
        var dbContext = Factory.GetDbContext();
        dbContext.ChartOfAccounts.AddRange(
            new ChartOfAccount { Id = Guid.NewGuid(), AccountNumber = "1010", Name = "Bank", Type = AccountType.Asset, IsActive = true },
            new ChartOfAccount { Id = Guid.NewGuid(), AccountNumber = "3010", Name = "Equity", Type = AccountType.Equity, IsActive = true }
        );
        await dbContext.SaveChangesAsync();

        var csvContent = new StringBuilder();
        csvContent.AppendLine("AccountNumber,DebitAmount,CreditAmount,Description");
        csvContent.AppendLine("1010,1000.00,0.00,Opening bank");
        csvContent.AppendLine("3010,0.00,1000.00,Opening equity");

        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(csvContent.ToString()));
        var formData = new MultipartFormDataContent();
        formData.Add(fileContent, "file", "balances.csv");

        // Act
        var response = await Client.PostAsync("/accounting/v1/bulk-import/opening-balances", formData);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<BulkImportResult>();
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(2, result.ImportedRecords);

        var entry = await dbContext.JournalEntries.Include(e => e.Lines).FirstOrDefaultAsync(e => e.Description == "Opening Balances Import");
        Assert.NotNull(entry);
        Assert.Equal(2, entry.Lines.Count);
        Assert.Equal(1000m, entry.TotalDebit);
        Assert.Equal(1000m, entry.TotalCredit);
    }

    [Fact]
    public async Task ImportOpeningBalances_Unbalanced_ShouldFail()
    {
        await CleanDatabaseAsync();

        // Arrange - Seed accounts
        var dbContext = Factory.GetDbContext();
        dbContext.ChartOfAccounts.AddRange(
            new ChartOfAccount { Id = Guid.NewGuid(), AccountNumber = "1020", Name = "Bank", Type = AccountType.Asset, IsActive = true },
            new ChartOfAccount { Id = Guid.NewGuid(), AccountNumber = "3020", Name = "Equity", Type = AccountType.Equity, IsActive = true }
        );
        await dbContext.SaveChangesAsync();

        var csvContent = "AccountNumber,DebitAmount,CreditAmount\n1020,1000,0\n3020,0,500"; // Unbalanced
        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(csvContent));
        var formData = new MultipartFormDataContent();
        formData.Add(fileContent, "file", "balances.csv");

        // Act
        var response = await Client.PostAsync("/accounting/v1/bulk-import/opening-balances", formData);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<BulkImportResult>();
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Contains(result.Errors, e => e.Contains("not balanced"));
    }

    [Fact]
    public async Task ImportOpeningBalances_MissingAccount_ShouldFail()
    {
        await CleanDatabaseAsync();

        // Arrange
        var csvContent = "AccountNumber,DebitAmount,CreditAmount\n9999,100,100"; // Account 9999 doesn't exist
        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(csvContent));
        var formData = new MultipartFormDataContent();
        formData.Add(fileContent, "file", "balances.csv");

        // Act
        var response = await Client.PostAsync("/accounting/v1/bulk-import/opening-balances", formData);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<BulkImportResult>();
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Contains(result.Errors, e => e.Contains("Account not found"));
    }

    [Fact]
    public async Task ImportChartOfAccounts_ValidationErrors_ShouldFail()
    {
        await CleanDatabaseAsync();

        // Arrange - Missing Name
        var csvContent = "AccountNumber,Name,Type,Category,IsActive\n7000,,Asset,Category,True";
        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(csvContent));
        var formData = new MultipartFormDataContent();
        formData.Add(fileContent, "file", "accounts.csv");

        // Act
        var response = await Client.PostAsync("/accounting/v1/bulk-import/chart-of-accounts", formData);

        // Assert
        var result = await response.Content.ReadFromJsonAsync<BulkImportResult>();
        Assert.False(result!.Success);
        Assert.Contains(result.Errors, e => e.Contains("missing name"));
    }

    [Fact]
    public async Task ImportChartOfAccounts_DuplicateInFile_ShouldFail()
    {
        await CleanDatabaseAsync();

        // Arrange - Duplicate account number in same file
        var csvContent = "AccountNumber,Name,Type,Category,IsActive\n8000,Account 1,Asset,Category,True\n8000,Account 2,Asset,Category,True";
        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(csvContent));
        var formData = new MultipartFormDataContent();
        formData.Add(fileContent, "file", "accounts.csv");

        // Act
        var response = await Client.PostAsync("/accounting/v1/bulk-import/chart-of-accounts", formData);

        // Assert
        var result = await response.Content.ReadFromJsonAsync<BulkImportResult>();
        Assert.False(result!.Success);
        Assert.Contains(result.Errors, e => e.Contains("Duplicate account number"));
    }

    [Fact]
    public async Task ImportOpeningBalances_Json_ShouldSucceed()
    {
        await CleanDatabaseAsync();

        // Arrange
        var dbContext = Factory.GetDbContext();
        dbContext.ChartOfAccounts.AddRange(
            new ChartOfAccount { Id = Guid.NewGuid(), AccountNumber = "1030", Name = "Bank", Type = AccountType.Asset, IsActive = true },
            new ChartOfAccount { Id = Guid.NewGuid(), AccountNumber = "3030", Name = "Equity", Type = AccountType.Equity, IsActive = true }
        );
        await dbContext.SaveChangesAsync();

        var jsonContent = JsonSerializer.Serialize(new[]
        {
            new { AccountNumber = "1030", DebitAmount = 2000m, CreditAmount = 0m, Description = "JSON Open" },
            new { AccountNumber = "3030", DebitAmount = 0m, CreditAmount = 2000m, Description = "JSON Open" }
        });

        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(jsonContent));
        var formData = new MultipartFormDataContent();
        formData.Add(fileContent, "file", "balances.json");

        // Act
        var response = await Client.PostAsync("/accounting/v1/bulk-import/opening-balances", formData);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<BulkImportResult>();
        Assert.True(result!.Success);
    }
}
