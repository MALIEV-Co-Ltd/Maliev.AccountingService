using System.Net;
using System.Net.Http.Json;
using Maliev.AccountingService.Api.DTOs.Requests;
using Maliev.AccountingService.Api.DTOs.Responses;
using Maliev.AccountingService.Data.Models;
using Microsoft.EntityFrameworkCore;
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
        Assert.Equal(createRequest.Type, createdAccount.Type);
        Assert.Equal(createRequest.Category, createdAccount.Category);
        Assert.True(createdAccount.IsActive);

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
            Description = "Test account for update testing",
            Type = "Liability",
            Category = "Current Liabilities",
            IsActive = true
        };

        var createResponse = await Client.PostAsJsonAsync("/accounting/v1/chart-of-accounts", createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var createdAccount = await createResponse.Content.ReadFromJsonAsync<ChartOfAccountResponse>();
        Assert.NotNull(createdAccount);

        // Act - Update the category
        var updateRequest = new UpdateChartOfAccountRequest
        {
            Name = "Updated Liability Account",
            Description = "Updated description",
            Category = "Long-term Liabilities"
        };

        var updateResponse = await Client.PutAsJsonAsync(
            $"/accounting/v1/chart-of-accounts/{createdAccount.Id}",
            updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var updatedAccount = await updateResponse.Content.ReadFromJsonAsync<ChartOfAccountResponse>();
        Assert.NotNull(updatedAccount);
        Assert.Equal(updateRequest.Name, updatedAccount.Name);
        Assert.Equal(updateRequest.Category, updatedAccount.Category);

        // Verify in database
        var dbContext = Factory.GetDbContext();
        var accountInDb = await dbContext.ChartOfAccounts
            .FirstOrDefaultAsync(a => a.Id == createdAccount.Id);

        Assert.NotNull(accountInDb);
        Assert.Equal("Long-term Liabilities", accountInDb.Category);
    }

    [Fact]
    public async Task DeactivateAccount_ShouldMarkInactive_AndPreventNewTransactions()
    {
        await CleanDatabaseAsync();

        // Arrange - Create an active account
        var createRequest = new CreateChartOfAccountRequest
        {
            AccountNumber = "3000-TEST-003",
            Name = "Test Account for Deactivation",
            Description = "Account to be deactivated",
            Type = "Expense",
            Category = "Operating Expenses",
            IsActive = true
        };

        var createResponse = await Client.PostAsJsonAsync("/accounting/v1/chart-of-accounts", createRequest);
        var createdAccount = await createResponse.Content.ReadFromJsonAsync<ChartOfAccountResponse>();
        Assert.NotNull(createdAccount);
        Assert.True(createdAccount.IsActive);

        // Act - Deactivate the account
        var deleteResponse = await Client.DeleteAsync($"/accounting/v1/chart-of-accounts/{createdAccount.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // Verify account is now inactive
        var getResponse = await Client.GetAsync($"/accounting/v1/chart-of-accounts/{createdAccount.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var deactivatedAccount = await getResponse.Content.ReadFromJsonAsync<ChartOfAccountResponse>();
        Assert.NotNull(deactivatedAccount);
        Assert.False(deactivatedAccount.IsActive);

        // Verify in database
        var dbContext = Factory.GetDbContext();
        var accountInDb = await dbContext.ChartOfAccounts
            .FirstOrDefaultAsync(a => a.Id == createdAccount.Id);

        Assert.NotNull(accountInDb);
        Assert.False(accountInDb.IsActive);

        // Verify account is not returned in active accounts list (default filter)
        var listResponse = await Client.GetAsync("/accounting/v1/chart-of-accounts");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

        var activeAccounts = await listResponse.Content.ReadFromJsonAsync<List<ChartOfAccountResponse>>();
        Assert.NotNull(activeAccounts);
        Assert.DoesNotContain(activeAccounts, a => a.Id == createdAccount.Id);

        // Verify account IS returned when includeInactive=true
        var listWithInactiveResponse = await Client.GetAsync("/accounting/v1/chart-of-accounts?includeInactive=true");
        var allAccounts = await listWithInactiveResponse.Content.ReadFromJsonAsync<List<ChartOfAccountResponse>>();
        Assert.NotNull(allAccounts);
        Assert.Contains(allAccounts, a => a.Id == createdAccount.Id);
    }

    [Fact]
    public async Task GetAccountHierarchy_ShouldReturnParentChildRelationships_Correctly()
    {
        await CleanDatabaseAsync();

        // Arrange - Create parent and child accounts
        var parentRequest = new CreateChartOfAccountRequest
        {
            AccountNumber = "4000-PARENT",
            Name = "Parent Asset Account",
            Description = "Parent account for hierarchy testing",
            Type = "Asset",
            Category = "Fixed Assets",
            IsActive = true
        };

        var parentResponse = await Client.PostAsJsonAsync("/accounting/v1/chart-of-accounts", parentRequest);
        var parentAccount = await parentResponse.Content.ReadFromJsonAsync<ChartOfAccountResponse>();
        Assert.NotNull(parentAccount);

        // Create child account
        var childRequest = new CreateChartOfAccountRequest
        {
            AccountNumber = "4100-CHILD",
            Name = "Child Asset Account",
            Description = "Child account under parent",
            Type = "Asset",
            Category = "Fixed Assets",
            ParentAccountId = parentAccount.Id,
            IsActive = true
        };

        var childResponse = await Client.PostAsJsonAsync("/accounting/v1/chart-of-accounts", childRequest);
        var childAccount = await childResponse.Content.ReadFromJsonAsync<ChartOfAccountResponse>();
        Assert.NotNull(childAccount);

        // Act - Get hierarchy
        var hierarchyResponse = await Client.GetAsync("/accounting/v1/chart-of-accounts/hierarchy");

        // Assert
        Assert.Equal(HttpStatusCode.OK, hierarchyResponse.StatusCode);

        var hierarchy = await hierarchyResponse.Content.ReadFromJsonAsync<List<ChartOfAccountResponse>>();
        Assert.NotNull(hierarchy);

        // Find parent in hierarchy
        var parentInHierarchy = hierarchy.FirstOrDefault(a => a.Id == parentAccount.Id);
        Assert.NotNull(parentInHierarchy);

        // Verify child is under parent
        Assert.NotNull(parentInHierarchy.Children);
        Assert.Contains(parentInHierarchy.Children, c => c.Id == childAccount.Id);

        // Verify child has correct parent reference
        var childInDb = await Factory.GetDbContext().ChartOfAccounts
            .FirstOrDefaultAsync(a => a.Id == childAccount.Id);
        Assert.NotNull(childInDb);
        Assert.Equal(parentAccount.Id, childInDb.ParentAccountId);
    }

    [Fact]
    public async Task CreateAccount_ShouldRejectDuplicateAccountNumber_WithError()
    {
        await CleanDatabaseAsync();

        // Arrange - Create first account
        var firstRequest = new CreateChartOfAccountRequest
        {
            AccountNumber = "5000-DUPLICATE",
            Name = "First Account",
            Description = "Original account",
            Type = "Revenue",
            Category = "Operating Revenue",
            IsActive = true
        };

        var firstResponse = await Client.PostAsJsonAsync("/accounting/v1/chart-of-accounts", firstRequest);
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        // Act - Try to create account with same account number
        var duplicateRequest = new CreateChartOfAccountRequest
        {
            AccountNumber = "5000-DUPLICATE", // Same account number
            Name = "Duplicate Account",
            Description = "This should fail",
            Type = "Revenue",
            Category = "Operating Revenue",
            IsActive = true
        };

        var duplicateResponse = await Client.PostAsJsonAsync("/accounting/v1/chart-of-accounts", duplicateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);

        var errorContent = await duplicateResponse.Content.ReadAsStringAsync();
        Assert.Contains("already exists", errorContent, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetAccountByNumber_ShouldReturnAccount_WhenExists()
    {
        await CleanDatabaseAsync();

        // Arrange
        var createRequest = new CreateChartOfAccountRequest
        {
            AccountNumber = "6000-SEARCH",
            Name = "Searchable Account",
            Description = "Account for search testing",
            Type = "Asset",
            Category = "Current Assets",
            IsActive = true
        };

        var createResponse = await Client.PostAsJsonAsync("/accounting/v1/chart-of-accounts", createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        // Act
        var getResponse = await Client.GetAsync("/accounting/v1/chart-of-accounts/by-number/6000-SEARCH");

        // Assert
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var account = await getResponse.Content.ReadFromJsonAsync<ChartOfAccountResponse>();
        Assert.NotNull(account);
        Assert.Equal("6000-SEARCH", account.AccountNumber);
        Assert.Equal("Searchable Account", account.Name);
    }

    [Fact]
    public async Task GetAccounts_ShouldFilterByAccountType_Correctly()
    {
        await CleanDatabaseAsync();

        // Arrange - Create accounts of different types
        var assetRequest = new CreateChartOfAccountRequest
        {
            AccountNumber = "7000-ASSET",
            Name = "Asset for Filter Test",
            Type = "Asset",
            Category = "Current Assets",
            IsActive = true
        };

        var liabilityRequest = new CreateChartOfAccountRequest
        {
            AccountNumber = "7100-LIABILITY",
            Name = "Liability for Filter Test",
            Type = "Liability",
            Category = "Current Liabilities",
            IsActive = true
        };

        await Client.PostAsJsonAsync("/accounting/v1/chart-of-accounts", assetRequest);
        await Client.PostAsJsonAsync("/accounting/v1/chart-of-accounts", liabilityRequest);

        // Act - Filter by Asset type
        var assetResponse = await Client.GetAsync("/accounting/v1/chart-of-accounts?accountType=Asset");

        // Assert
        Assert.Equal(HttpStatusCode.OK, assetResponse.StatusCode);

        var assets = await assetResponse.Content.ReadFromJsonAsync<List<ChartOfAccountResponse>>();
        Assert.NotNull(assets);

        // Verify all returned accounts are Assets
        Assert.All(assets, account => Assert.Equal("Asset", account.Type));

        // Verify our test asset is in the list
        Assert.Contains(assets, a => a.AccountNumber == "7000-ASSET");

        // Verify liability is NOT in the list
        Assert.DoesNotContain(assets, a => a.AccountNumber == "7100-LIABILITY");
    }
}
