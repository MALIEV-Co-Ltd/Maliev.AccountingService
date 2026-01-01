using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Microsoft.EntityFrameworkCore;
using Maliev.AccountingService.Data.Data;
using Microsoft.AspNetCore.Authorization;

namespace Maliev.AccountingService.Api.Controllers;

/// <summary>
/// Controller for viewing accounting-related permissions and roles
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("accounting/v{version:apiVersion}/permissions")]
public class PermissionsController : ControllerBase
{
    private readonly AccountingDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="PermissionsController"/> class.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    public PermissionsController(AccountingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// List all registered permissions
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetPermissions()
    {
        var permissions = await _dbContext.Permissions
            .Select(p => new
            {
                p.Code,
                p.Description,
                p.IsCritical
            })
            .ToListAsync();

        return Ok(permissions);
    }

    /// <summary>
    /// List all predefined roles and their permissions
    /// </summary>
    [HttpGet("roles")]
    public async Task<IActionResult> GetRoles()
    {
        var roles = await _dbContext.Roles
            .Select(r => new
            {
                r.Name,
                r.Description,
                Permissions = r.RolePermissions.Select(rp => rp.PermissionCode)
            })
            .ToListAsync();

        return Ok(roles);
    }
}
