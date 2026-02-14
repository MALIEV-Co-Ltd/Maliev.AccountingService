using Maliev.AccountingService.Data.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Maliev.AccountingService.Tests.Infrastructure;

/// <summary>Integrity tests.</summary>
public class ModelIntegrityTests
{
    /// <summary>Check for pending migrations.</summary>
    [Fact]
    public void Model_ShouldNotHavePendingChanges()
    {
        var options = new DbContextOptionsBuilder<AccountingDbContext>()
            .UseNpgsql("Host=localhost;Database=ModelCheck")
            .Options;

        using var context = new AccountingDbContext(options);
        var hasChanges = context.Database.HasPendingModelChanges();

        Assert.False(hasChanges, "Run 'dotnet ef migrations add <Name> --project Maliev.AccountingService.Data --startup-project Maliev.AccountingService.Api'");
    }
}
