using System.ComponentModel.DataAnnotations;

namespace Maliev.AccountingService.Data.Models;

public class RolePermission
{
    [Required]
    [MaxLength(50)]
    public string RoleName { get; set; } = default!;
    public Role Role { get; set; } = default!;

    [Required]
    [MaxLength(100)]
    public string PermissionCode { get; set; } = default!;
    public Permission Permission { get; set; } = default!;
}
