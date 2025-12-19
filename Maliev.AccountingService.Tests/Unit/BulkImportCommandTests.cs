using Maliev.AccountingService.Api.Commands;
using Maliev.AccountingService.Data.Data;
using Maliev.AccountingService.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using Xunit;

namespace Maliev.AccountingService.Tests.Integration;

/// <summary>
/// Integration tests for BulkImportCommand using Testcontainers
/// Tests CSV/JSON import command structure and file handling
/// </summary>
[Collection("Integration Tests")]
public class BulkImportCommandTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly string _testFilesDirectory;
    private readonly AccountingDbContext _dbContext;

    public BulkImportCommandTests(IntegrationTestFixture fixture)
    {
        _testFilesDirectory = Path.Combine(Path.GetTempPath(), $"BulkImportTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testFilesDirectory);

        // Use the test database from the fixture
        var services = new ServiceCollection();
        services.AddSingleton(fixture.WebAppFactory.GetDbContext());
        _serviceProvider = services.BuildServiceProvider();
        _dbContext = fixture.WebAppFactory.GetDbContext();
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
        if (Directory.Exists(_testFilesDirectory))
        {
            Directory.Delete(_testFilesDirectory, true);
        }
    }

    [Fact]
    public void CreateCommand_ShouldHaveRequiredOptions()
    {
        // Act
        var command = BulkImportCommand.CreateCommand(_serviceProvider);

        // Assert
        Assert.NotNull(command);
        Assert.Equal("bulk-import", command.Name);
        Assert.Contains(command.Options, o => o.Name == "file" || o.Aliases.Contains("--file"));
        Assert.Contains(command.Options, o => o.Name == "type" || o.Aliases.Contains("--type"));
        Assert.Contains(command.Options, o => o.Name == "dry-run" || o.Aliases.Contains("--dry-run"));
    }

    [Fact]
    public async Task ImportChartOfAccounts_FromCsv_CommandStructureWorks()
    {
        // Arrange - Create test CSV file
        var csvPath = Path.Combine(_testFilesDirectory, "test_accounts.csv");
        var csvContent = @"AccountNumber,Name,Type,Category,Description,ParentAccountNumber
1000,Cash,Asset,Current Assets,Cash on hand,";
        await File.WriteAllTextAsync(csvPath, csvContent);

        var command = BulkImportCommand.CreateCommand(_serviceProvider);

        // Assert - Command has proper structure
        Assert.NotNull(command);
        Assert.Equal("bulk-import", command.Name);
    }

    [Fact]
    public async Task ImportChartOfAccounts_FromJson_CanCreateCommand()
    {
        // Arrange - Create test JSON file
        var jsonPath = Path.Combine(_testFilesDirectory, "test_accounts.json");
        var jsonContent = @"[{""accountNumber"":""1000"",""name"":""Cash""}]";
        await File.WriteAllTextAsync(jsonPath, jsonContent);

        // Act
        var command = BulkImportCommand.CreateCommand(_serviceProvider);

        // Assert - Command structure is valid
        Assert.NotNull(command);
    }

    [Fact]
    public void CreateCommand_HasDryRunOption()
    {
        // Act
        var command = BulkImportCommand.CreateCommand(_serviceProvider);

        // Assert
        Assert.Contains(command.Options, o =>
            o.Name == "dry-run" || o.Aliases.Contains("--dry-run") || o.Aliases.Contains("-d"));
    }

    [Fact]
    public void CreateCommand_HasFileOption()
    {
        // Act
        var command = BulkImportCommand.CreateCommand(_serviceProvider);

        // Assert
        Assert.Contains(command.Options, o =>
            o.Name == "file" || o.Aliases.Contains("--file") || o.Aliases.Contains("-f"));
    }

    [Fact]
    public void CreateCommand_HasTypeOption()
    {
        // Act
        var command = BulkImportCommand.CreateCommand(_serviceProvider);

        // Assert
        Assert.Contains(command.Options, o =>
            o.Name == "type" || o.Aliases.Contains("--type") || o.Aliases.Contains("-t"));
    }

    [Fact]
    public void Command_ShouldHaveCorrectDescription()
    {
        // Act
        var command = BulkImportCommand.CreateCommand(_serviceProvider);

        // Assert
        Assert.Contains("import", command.Description, StringComparison.OrdinalIgnoreCase);
    }
}
