using Maliev.AccountingService.Api.Extensions;
using Maliev.AccountingService.Infrastructure.Models;
using Xunit;

namespace Maliev.AccountingService.Tests.Unit;

/// <summary>
/// Unit tests for ChartOfAccountExtensions
/// </summary>
public class ChartOfAccountExtensionsTests
{
    [Fact]
    public void ToResponse_ShouldMapAccountCorrectly()
    {
        // Arrange
        var account = new ChartOfAccount
        {
            Id = Guid.NewGuid(),
            AccountNumber = "1000",
            Name = "Cash",
            Type = AccountType.Asset,
            Category = "Current Assets",
            Description = "Cash account",
            IsActive = true,
            ParentAccountId = null
        };

        // Act
        var response = account.ToResponse();

        // Assert
        Assert.NotNull(response);
        Assert.Equal(account.Id, response.Id);
        Assert.Equal("1000", response.AccountNumber);
        Assert.Equal("Cash", response.Name);
        Assert.Equal("Asset", response.Type);
        Assert.Equal("Current Assets", response.Category);
        Assert.True(response.IsActive);
    }

    [Fact]
    public void ToResponse_WithParent_ShouldSetParentAccountNumber()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var account = new ChartOfAccount
        {
            Id = Guid.NewGuid(),
            AccountNumber = "1000-001",
            Name = "Petty Cash",
            Type = AccountType.Asset,
            Category = "Current Assets",
            IsActive = true,
            ParentAccountId = parentId,
            ParentAccount = new ChartOfAccount
            {
                Id = parentId,
                AccountNumber = "1000",
                Name = "Cash"
            }
        };

        // Act
        var response = account.ToResponse();

        // Assert
        Assert.NotNull(response);
        Assert.Equal("1000", response.ParentAccountNumber);
    }

    [Fact]
    public void ToResponse_WithoutParent_ShouldReturnNullParentAccountNumber()
    {
        // Arrange
        var account = new ChartOfAccount
        {
            Id = Guid.NewGuid(),
            AccountNumber = "1000",
            Name = "Cash",
            Type = AccountType.Asset,
            Category = "Current Assets",
            IsActive = true,
            ParentAccountId = null
        };

        // Act
        var response = account.ToResponse();

        // Assert
        Assert.NotNull(response);
        Assert.Null(response.ParentAccountNumber);
    }

    [Fact]
    public void ToResponse_ShouldHandleAllAccountTypes()
    {
        var types = new[] { AccountType.Asset, AccountType.Liability, AccountType.Equity, AccountType.Revenue, AccountType.Expense };

        foreach (var type in types)
        {
            // Arrange
            var account = new ChartOfAccount
            {
                Id = Guid.NewGuid(),
                AccountNumber = $"{(int)type:D4}",
                Name = type.ToString(),
                Type = type,
                Category = "Test",
                IsActive = true
            };

            // Act
            var response = account.ToResponse();

            // Assert
            Assert.NotNull(response);
            Assert.Equal(type.ToString(), response.Type);
        }
    }

    [Fact]
    public void ToResponse_ShouldMapChildrenCorrectly()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var childId = Guid.NewGuid();
        var parent = new ChartOfAccount
        {
            Id = parentId,
            AccountNumber = "1000",
            Name = "Cash",
            Type = AccountType.Asset,
            Category = "Current Assets",
            IsActive = true,
            ChildAccounts = new List<ChartOfAccount>
            {
                new ChartOfAccount
                {
                    Id = childId,
                    AccountNumber = "1000-001",
                    Name = "Petty Cash",
                    Type = AccountType.Asset,
                    Category = "Current Assets",
                    IsActive = true,
                    ParentAccountId = parentId
                }
            }
        };

        // Act
        var response = parent.ToResponse();

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Children);
        var childResponse = Assert.Single(response.Children);
        Assert.Equal(childId, childResponse.Id);
        Assert.Equal("Petty Cash", childResponse.Name);
    }
}
