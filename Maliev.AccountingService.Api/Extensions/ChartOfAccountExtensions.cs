using Maliev.AccountingService.Api.DTOs.Requests;
using Maliev.AccountingService.Api.DTOs.Responses;
using Maliev.AccountingService.Infrastructure.Models;

namespace Maliev.AccountingService.Api.Extensions;

/// <summary>
/// Extension methods for mapping between ChartOfAccount entity and DTOs
/// </summary>
public static class ChartOfAccountExtensions
{
    /// <summary>
    /// Convert ChartOfAccount entity to response DTO
    /// </summary>
    public static ChartOfAccountResponse ToResponse(this ChartOfAccount account)
    {
        return new ChartOfAccountResponse
        {
            Id = account.Id,
            AccountNumber = account.AccountNumber,
            Name = account.Name,
            Description = account.Description,
            Type = account.Type.ToString(),
            Category = account.Category,
            ParentAccountId = account.ParentAccountId,
            ParentAccountNumber = account.ParentAccount?.AccountNumber,
            ParentAccountName = account.ParentAccount?.Name,
            IsActive = account.IsActive,
            CreatedAt = account.CreatedAt,
            ModifiedAt = account.ModifiedAt,
            Children = account.ChildAccounts?.Select(c => c.ToResponse()).ToList()
        };
    }

    /// <summary>
    /// Convert collection of ChartOfAccount entities to response DTOs
    /// </summary>
    public static List<ChartOfAccountResponse> ToResponse(this IEnumerable<ChartOfAccount> accounts)
    {
        return accounts.Select(a => a.ToResponse()).ToList();
    }

    /// <summary>
    /// Convert create request DTO to ChartOfAccount entity
    /// </summary>
    public static ChartOfAccount ToEntity(this CreateChartOfAccountRequest request)
    {
        if (!Enum.TryParse<AccountType>(request.Type, out var accountType))
        {
            throw new ArgumentException($"Invalid account type: {request.Type}", nameof(request.Type));
        }

        return new ChartOfAccount
        {
            Id = Guid.NewGuid(),
            AccountNumber = request.AccountNumber,
            Name = request.Name,
            Description = request.Description,
            Type = accountType,
            Category = request.Category,
            ParentAccountId = request.ParentAccountId,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Apply update request DTO to existing ChartOfAccount entity
    /// </summary>
    public static void ApplyUpdate(this ChartOfAccount account, UpdateChartOfAccountRequest request)
    {
        account.Name = request.Name;
        account.Description = request.Description;
        account.Category = request.Category;
        account.ParentAccountId = request.ParentAccountId;
        account.ModifiedAt = DateTime.UtcNow;
    }
}
