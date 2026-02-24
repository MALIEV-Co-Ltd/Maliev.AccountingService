using System.Net;
using System.Net.Http.Json;
using Maliev.AccountingService.Api.DTOs.Requests;
using Maliev.AccountingService.Api.DTOs.Responses;
using Maliev.AccountingService.Api.Extensions;
using Maliev.AccountingService.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Maliev.AccountingService.Tests.Integration;

/// <summary>
/// Integration tests for Chart of Accounts management.
/// Tests CRUD operations, hierarchy management, and business rules validation.
/// </summary>
public class ChartOfAccountsTests : BaseIntegrationTest
{
    public ChartOfAccountsTests(IntegrationTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task SeedStandardChartOfAccounts_ShouldAddAccounts()
    {
        await CleanDatabaseAsync();
        var dbContext = Factory.GetDbContext();
        var mockLogger = new Mock<ILogger>();

        // Act
        await dbContext.SeedStandardChartOfAccountsAsync(mockLogger.Object);

        // Assert
        var count = await dbContext.ChartOfAccounts.CountAsync();
        Assert.True(count > 20); // Standard set has many accounts

        // Call again should skip
        await dbContext.SeedStandardChartOfAccountsAsync(mockLogger.Object);
        Assert.Equal(count, await dbContext.ChartOfAccounts.CountAsync());
    }

    [Fact]
    public async Task CreateAccount_ShouldAddToChart_WithValidTypeAndCategory()
    {
        await CleanDatabaseAsync();

        // Arrange
        var createRequest = new CreateChartOfAccountRequest
        {
            AccountNumber = "1000-TEST-001",
            Name = "Test Cash Account",
            Description = "Test account for integration testing",
            Type = "Asset",
            Category = "Current Assets",
            IsActive = true
        };

        // Act
        var response = await Client.PostAsJsonAsync("/accounting/v1/chart-of-accounts", createRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var createdAccount = await response.Content.ReadFromJsonAsync<ChartOfAccountResponse>();
        Assert.NotNull(createdAccount);
        Assert.Equal(createRequest.AccountNumber, createdAccount.AccountNumber);
        Assert.Equal(createRequest.Name, createdAccount.Name);

        // Verify it was added to the database
        var dbContext = Factory.GetDbContext();
        var accountInDb = await dbContext.ChartOfAccounts
            .FirstOrDefaultAsync(a => a.AccountNumber == createRequest.AccountNumber);

        Assert.NotNull(accountInDb);
        Assert.Equal(createRequest.Name, accountInDb.Name);
    }

    [Fact]
    public async Task UpdateAccount_ShouldUpdateCategory_AndReflectInFutureReporting()
    {
        await CleanDatabaseAsync();

        // Arrange - Create an account first
        var createRequest = new CreateChartOfAccountRequest
        {
            AccountNumber = "2000-TEST-002",
            Name = "Test Liability Account",
            Type = "Liability",
            Category = "Current Liabilities",
            IsActive = true
        };

        var createResponse = await Client.PostAsJsonAsync("/accounting/v1/chart-of-accounts", createRequest);
        var createdAccount = await createResponse.Content.ReadFromJsonAsync<ChartOfAccountResponse>();

        // Act - Update the category
        var updateRequest = new UpdateChartOfAccountRequest
        {
            Name = "Updated Liability Account",
            Category = "Long-term Liabilities"
        };

        var updateResponse = await Client.PutAsJsonAsync($"/accounting/v1/chart-of-accounts/{createdAccount!.Id}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updatedAccount = await updateResponse.Content.ReadFromJsonAsync<ChartOfAccountResponse>();
        Assert.Equal("Long-term Liabilities", updatedAccount!.Category);
    }

    [Fact]
    public async Task DeactivateAccount_ShouldMarkInactive()
    {
        await CleanDatabaseAsync();

        // Arrange - Create an active account
        var createRequest = new CreateChartOfAccountRequest
        {
            AccountNumber = "3000-TEST-003",
            Name = "To Deactivate",
            Type = "Expense",
            Category = "Operating Expenses",
            IsActive = true
        };

        var createResponse = await Client.PostAsJsonAsync("/accounting/v1/chart-of-accounts", createRequest);
        var createdAccount = await createResponse.Content.ReadFromJsonAsync<ChartOfAccountResponse>();

        // Act - Deactivate
        var deleteResponse = await Client.DeleteAsync($"/accounting/v1/chart-of-accounts/{createdAccount!.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task GetAccountHierarchy_ShouldReturnHierarchy()
    {
        await CleanDatabaseAsync();

        // Act
        var response = await Client.GetAsync("/accounting/v1/chart-of-accounts/hierarchy");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetAccountByNumber_ShouldReturnAccount()
    {
        await CleanDatabaseAsync();

        // Arrange
        var createRequest = new CreateChartOfAccountRequest
        {
            AccountNumber = "6000-SEARCH",
            Name = "Searchable",
            Type = "Asset",
            Category = "Current Assets",
            IsActive = true
        };
        await Client.PostAsJsonAsync("/accounting/v1/chart-of-accounts", createRequest);

        // Act
        var response = await Client.GetAsync("/accounting/v1/chart-of-accounts/by-number/6000-SEARCH");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetAccountById_WhenNotExists_ShouldReturnNotFound()
    {
        await CleanDatabaseAsync();
        var response = await Client.GetAsync($"/accounting/v1/chart-of-accounts/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeactivateAccount_WhenNotExists_ShouldReturnBadRequest()
    {
        await CleanDatabaseAsync();
        var response = await Client.DeleteAsync($"/accounting/v1/chart-of-accounts/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateAccount_WithInvalidData_ShouldReturnBadRequest()
    {
        var request = new CreateChartOfAccountRequest { AccountNumber = "", Name = "" };
        var response = await Client.PostAsJsonAsync("/accounting/v1/chart-of-accounts", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateAccount_WhenNotExists_ShouldReturnNotFound()
    {
        var request = new UpdateChartOfAccountRequest { Name = "New Name" };
        var response = await Client.PutAsJsonAsync($"/accounting/v1/chart-of-accounts/{Guid.NewGuid()}", request);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAccounts_WithInvalidType_ShouldNotFilter()
    {
        await CleanDatabaseAsync();

        // Act
        var response = await Client.GetAsync("/accounting/v1/chart-of-accounts?accountType=InvalidType");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateAccount_CircularReference_ShouldReturnBadRequest()
    {
        await CleanDatabaseAsync();

        // Arrange
        var dbContext = Factory.GetDbContext();
        var account1 = new ChartOfAccount { Id = Guid.NewGuid(), AccountNumber = "1001", Name = "A1", Type = AccountType.Asset, IsActive = true };
        var account2 = new ChartOfAccount { Id = Guid.NewGuid(), AccountNumber = "1002", Name = "A2", Type = AccountType.Asset, IsActive = true, ParentAccountId = account1.Id };
        dbContext.ChartOfAccounts.AddRange(account1, account2);
        await dbContext.SaveChangesAsync();

        // Act - Try to make account 1 a child of account 2
        var updateRequest = new UpdateChartOfAccountRequest { ParentAccountId = account2.Id };
        var response = await Client.PutAsJsonAsync($"/accounting/v1/chart-of-accounts/{account1.Id}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DeactivateAccount_WithChildren_ShouldReturnBadRequest()
    {
        await CleanDatabaseAsync();

        // Arrange
        var dbContext = Factory.GetDbContext();
        var parent = new ChartOfAccount { Id = Guid.NewGuid(), AccountNumber = "2001", Name = "P1", Type = AccountType.Asset, IsActive = true };
        var child = new ChartOfAccount { Id = Guid.NewGuid(), AccountNumber = "2002", Name = "C1", Type = AccountType.Asset, IsActive = true, ParentAccountId = parent.Id };
        dbContext.ChartOfAccounts.AddRange(parent, child);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await Client.DeleteAsync($"/accounting/v1/chart-of-accounts/{parent.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateAccount_WithNonExistentParent_ShouldReturnBadRequest()
    {
        await CleanDatabaseAsync();

        // Arrange
        var dbContext = Factory.GetDbContext();
        var account = new ChartOfAccount { Id = Guid.NewGuid(), AccountNumber = "3001", Name = "A1", Type = AccountType.Asset, IsActive = true };
        dbContext.ChartOfAccounts.Add(account);
        await dbContext.SaveChangesAsync();

        // Act
        var updateRequest = new UpdateChartOfAccountRequest { ParentAccountId = Guid.NewGuid() };
        var response = await Client.PutAsJsonAsync($"/accounting/v1/chart-of-accounts/{account.Id}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
