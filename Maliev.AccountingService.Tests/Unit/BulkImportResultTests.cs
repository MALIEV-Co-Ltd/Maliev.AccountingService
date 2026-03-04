using Maliev.AccountingService.Api.Models;
using Maliev.AccountingService.Api.Services;
using Xunit;

namespace Maliev.AccountingService.Tests.Unit;

/// <summary>
/// Unit tests for IBulkImportService and related DTOs
/// </summary>
public class BulkImportResultTests
{
    [Fact]
    public void BulkImportResult_ShouldInitializeWithDefaults()
    {
        // Act
        var result = new BulkImportResult();

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Errors);
        Assert.Empty(result.Errors);
        Assert.NotNull(result.Warnings);
        Assert.Empty(result.Warnings);
        Assert.False(result.Success);
        Assert.Equal(0, result.TotalRecords);
        Assert.Equal(0, result.ImportedRecords);
        Assert.Equal(0, result.SkippedRecords);
    }

    [Fact]
    public void BulkImportResult_AddError_ShouldAddToErrorsList()
    {
        // Arrange
        var result = new BulkImportResult();

        // Act
        result.Errors.Add("Test error");

        // Assert
        Assert.Single(result.Errors);
        Assert.Equal("Test error", result.Errors[0]);
    }

    [Fact]
    public void BulkImportResult_AddWarning_ShouldAddToWarningsList()
    {
        // Arrange
        var result = new BulkImportResult();

        // Act
        result.Warnings.Add("Test warning");

        // Assert
        Assert.Single(result.Warnings);
    }
}
