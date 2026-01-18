using Maliev.AccountingService.Data.Models;
using Maliev.Aspire.ServiceDefaults.Database;
using Microsoft.EntityFrameworkCore;

namespace Maliev.AccountingService.Data.Data;

public class AccountingDbContext : DbContext
{
    public AccountingDbContext(DbContextOptions<AccountingDbContext> options)
        : base(options)
    {
    }

    // DbSets for all entities
    public DbSet<ChartOfAccount> ChartOfAccounts => Set<ChartOfAccount>();
    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
    public DbSet<JournalEntryLine> JournalEntryLines => Set<JournalEntryLine>();
    public DbSet<FinancialPeriod> FinancialPeriods => Set<FinancialPeriod>();
    public DbSet<FiscalYear> FiscalYears => Set<FiscalYear>();
    public DbSet<TaxComponent> TaxComponents => Set<TaxComponent>();
    public DbSet<SubledgerTransaction> SubledgerTransactions => Set<SubledgerTransaction>();
    public DbSet<AuditTrailEntry> AuditTrailEntries => Set<AuditTrailEntry>();
    public DbSet<ProcessedEventRegistry> ProcessedEventRegistry => Set<ProcessedEventRegistry>();
    public DbSet<ReconciliationReport> ReconciliationReports => Set<ReconciliationReport>();
    public DbSet<AdjustingEntryApproval> AdjustingEntryApprovals => Set<AdjustingEntryApproval>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Sequence for Journal Entry Numbers
        modelBuilder.HasSequence<long>("journal_entry_number_seq")
            .StartsAt(1)
            .IncrementsBy(1);

        // Configure ChartOfAccount
        modelBuilder.Entity<ChartOfAccount>(entity =>
        {
            entity.ToTable("chart_of_accounts");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.AccountNumber).IsUnique();
            entity.HasIndex(e => e.ParentAccountId);
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.IsActive).HasFilter("is_active = true");

            entity.HasOne(e => e.ParentAccount)
                .WithMany(e => e.ChildAccounts)
                .HasForeignKey(e => e.ParentAccountId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure FiscalYear
        modelBuilder.Entity<FiscalYear>(entity =>
        {
            entity.ToTable("fiscal_years");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasIndex(e => new { e.StartDate, e.EndDate });
        });

        // Configure FinancialPeriod
        modelBuilder.Entity<FinancialPeriod>(entity =>
        {
            entity.ToTable("financial_periods");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.FiscalYearId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => new { e.StartDate, e.EndDate });

            entity.HasOne(e => e.FiscalYear)
                .WithMany(e => e.FinancialPeriods)
                .HasForeignKey(e => e.FiscalYearId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure JournalEntry
        modelBuilder.Entity<JournalEntry>(entity =>
        {
            entity.ToTable("journal_entries");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.EntryNumber).IsUnique();
            entity.HasIndex(e => new { e.PeriodId, e.Status });
            entity.HasIndex(e => e.SourceEventId);
            entity.HasIndex(e => e.EntryDate);

            entity.HasOne(e => e.Period)
                .WithMany()
                .HasForeignKey(e => e.PeriodId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.AdjustingEntryApproval)
                .WithOne(e => e.JournalEntry)
                .HasForeignKey<AdjustingEntryApproval>(e => e.JournalEntryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure JournalEntryLine
        modelBuilder.Entity<JournalEntryLine>(entity =>
        {
            entity.ToTable("journal_entry_lines");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.JournalEntryId);
            entity.HasIndex(e => e.AccountId);
            entity.HasIndex(e => new { e.CustomerId });
            entity.HasIndex(e => new { e.SupplierId });

            entity.HasOne(e => e.JournalEntry)
                .WithMany(e => e.Lines)
                .HasForeignKey(e => e.JournalEntryId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Account)
                .WithMany(e => e.JournalEntryLines)
                .HasForeignKey(e => e.AccountId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure TaxComponent
        modelBuilder.Entity<TaxComponent>(entity =>
        {
            entity.ToTable("tax_components");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.JournalEntryLineId);
            entity.HasIndex(e => e.TaxType);

            entity.HasOne(e => e.JournalEntryLine)
                .WithMany(e => e.TaxComponents)
                .HasForeignKey(e => e.JournalEntryLineId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure SubledgerTransaction
        modelBuilder.Entity<SubledgerTransaction>(entity =>
        {
            entity.ToTable("subledger_transactions");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.SourceSystem, e.SourceTransactionId }).IsUnique();
            entity.HasIndex(e => e.JournalEntryId);
            entity.HasIndex(e => e.TransactionDate);

            entity.HasOne(e => e.JournalEntry)
                .WithMany()
                .HasForeignKey(e => e.JournalEntryId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure AuditTrailEntry
        modelBuilder.Entity<AuditTrailEntry>(entity =>
        {
            entity.ToTable("audit_trail_entries");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.EntityType, e.EntityId, e.PerformedAt });
            entity.HasIndex(e => e.PerformedBy);
            entity.HasIndex(e => e.CorrelationId);
        });

        // Configure ProcessedEventRegistry
        modelBuilder.Entity<ProcessedEventRegistry>(entity =>
        {
            entity.ToTable("processed_event_registry");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.EventId).IsUnique();
            entity.HasIndex(e => e.ProcessedAt);
        });

        // Configure ReconciliationReport
        modelBuilder.Entity<ReconciliationReport>(entity =>
        {
            entity.ToTable("reconciliation_reports");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.PeriodId, e.ReconciliationType });
            entity.HasIndex(e => e.Status).HasFilter("status != 'Resolved'");

            entity.HasOne(e => e.Period)
                .WithMany()
                .HasForeignKey(e => e.PeriodId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure AdjustingEntryApproval
        modelBuilder.Entity<AdjustingEntryApproval>(entity =>
        {
            entity.ToTable("adjusting_entry_approvals");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.JournalEntryId).IsUnique();
            entity.HasIndex(e => e.Status);
        });

        // Configure Permission
        modelBuilder.Entity<Permission>(entity =>
        {
            entity.ToTable("permissions");
            entity.HasKey(e => e.Code);
            entity.Property(e => e.Code).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(255).IsRequired();
        });

        // Configure Role
        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("roles");
            entity.HasKey(e => e.Name);
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(255).IsRequired();
        });

        // Configure RolePermission
        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.ToTable("role_permissions");
            entity.HasKey(e => new { e.RoleName, e.PermissionCode });

            entity.HasOne(e => e.Role)
                .WithMany(e => e.RolePermissions)
                .HasForeignKey(e => e.RoleName)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Permission)
                .WithMany(e => e.RolePermissions)
                .HasForeignKey(e => e.PermissionCode)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Seed Authorization Data
        modelBuilder.Entity<Permission>().HasData(
            AccountingPermissions.AllWithDescriptions.Select(p => new Permission
            {
                Code = p.Key,
                Description = p.Value,
                IsCritical = AccountingPermissions.CriticalPermissions.Contains(p.Key)
            }));

        modelBuilder.Entity<Role>().HasData(
            AccountingPredefinedRoles.All.Select(r => new Role
            {
                Name = r.RoleId,
                Description = r.Description
            }));

        var rolePermissions = new List<RolePermission>();
        foreach (var role in AccountingPredefinedRoles.All)
        {
            foreach (var permission in role.Permissions)
            {
                rolePermissions.Add(new RolePermission
                {
                    RoleName = role.RoleId,
                    PermissionCode = permission
                });
            }
        }
        modelBuilder.Entity<RolePermission>().HasData(rolePermissions);

        // Apply PostgreSQL snake_case naming convention globally
        SnakeCaseNamingHelper.ApplySnakeCaseNaming(modelBuilder);

        // Global converter for UTC DateTime to prevent Npgsql.UnspecifiedKind exceptions
        var dateTimeConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>(
            v => v.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(v, DateTimeKind.Utc) : v.ToUniversalTime(),
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                {
                    property.SetValueConverter(dateTimeConverter);
                }
            }
        }
    }
}
