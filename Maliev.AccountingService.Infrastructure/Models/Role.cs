using System.ComponentModel.DataAnnotations;

namespace Maliev.AccountingService.Infrastructure.Models;

public class Role
{
    [Key]
    [MaxLength(50)]
    public string Name { get; set; } = default!;

    [Required]
    [MaxLength(255)]
    public string Description { get; set; } = default!;

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
