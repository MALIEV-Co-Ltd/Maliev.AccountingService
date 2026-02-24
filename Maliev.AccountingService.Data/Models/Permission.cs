using System.ComponentModel.DataAnnotations;

namespace Maliev.AccountingService.Data.Models;

public class Permission
{
    [Key]
    [MaxLength(100)]
    public string Code { get; set; } = default!;

    [Required]
    [MaxLength(255)]
    public string Description { get; set; } = default!;

    public bool IsCritical { get; set; }

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
