using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Maliev.AccountingService.Data.Data;

/// <summary>
/// Design-time factory for creating AccountingDbContext during migrations
/// </summary>
public class AccountingDbContextFactory : IDesignTimeDbContextFactory<AccountingDbContext>
{
    public AccountingDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AccountingDbContext>();

        // Use a connection string for migrations only
        // This won't be used at runtime
        optionsBuilder.UseNpgsql("Host=localhost;Database=accounting;Username=postgres;Password=postgres");

        return new AccountingDbContext(optionsBuilder.Options);
    }
}
