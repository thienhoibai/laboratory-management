using IAM.Application.Permissions;
using IAM.Application.Permissions.DTOs;
using IAM.Application.Roles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using IAM.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace IAM.Presentation.Controllers
{
    [ApiController]
    [Route("api/rbac")]
    [Authorize(Policy = "perm:User.Manage")]
    public class RbacController : ControllerBase
    {
        private readonly IPermissionQuery _perms;
        private readonly IRolePermissionService _rolePerms;
        private readonly IamDbContext _db;
        public RbacController(IPermissionQuery perms, IRolePermissionService rolePerms, IamDbContext db)
        {
            _perms = perms; _rolePerms = rolePerms; _db = db;
        }

        private static Guid GetActorId(ClaimsPrincipal user)
        {
            var id = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst("sub")?.Value;
            return id != null && Guid.TryParse(id, out var g) ? g : Guid.Empty;
        }

        [HttpGet("permission-groups")]
        public async Task<ActionResult<List<PermissionGroupDto>>> GetPermissionGroups(CancellationToken ct)
        {
            var groups = await _perms.GetGroupsAsync(ct);
            return Ok(groups);
        }

        [HttpGet("roles")]
        public async Task<IActionResult> GetRoles(CancellationToken ct)
        {
            var roles = await _db.Roles.OrderBy(r => r.Name).Select(r => new { r.RoleId, r.Name }).ToListAsync(ct);
            return Ok(roles);
        }

        [HttpGet("roles/{roleId:int}/permissions")]
        public async Task<IActionResult> GetRolePermissions(int roleId, CancellationToken ct)
        {
            var keys = await _rolePerms.GetPermissionKeysByRoleAsync(roleId, ct);
            return Ok(keys);
        }

        public record UpdateRolePermissionsRequest(string[] PermissionKeys);

        [HttpPut("roles/{roleId:int}/permissions")]
        public async Task<IActionResult> UpdateRolePermissions(int roleId, [FromBody] UpdateRolePermissionsRequest body, CancellationToken ct)
        {
            var actorId = GetActorId(User);
            await _rolePerms.UpdateRolePermissionsByKeysAsync(roleId, body.PermissionKeys ?? Array.Empty<string>(), actorId, ct);
            return Ok(new { updated = true });
        }

        // PATCH delta add/remove keys to avoid sending full list
        public record UpdateRolePermissionsDeltaRequest(string[]? AddKeys, string[]? RemoveKeys);

        [HttpPatch("roles/{roleId:int}/permissions")]
        public async Task<IActionResult> PatchRolePermissions(int roleId, [FromBody] UpdateRolePermissionsDeltaRequest body, CancellationToken ct)
        {
            var actorId = GetActorId(User);
            var add = body?.AddKeys ?? Array.Empty<string>();
            var remove = body?.RemoveKeys ?? Array.Empty<string>();
            await _rolePerms.UpdateRolePermissionsDeltaByKeysAsync(roleId, add, remove, actorId, ct);
            return Ok(new { updated = true, added = add, removed = remove });
        }

        // NEW: Toggle all permissions of a module (e.g., Blog.*) for a role
        public record ToggleModulePermissionsRequest(bool Enable);

        [HttpPatch("roles/{roleId:int}/permissions/modules/{module}")]
        public async Task<IActionResult> ToggleModulePermissions(int roleId, string module, [FromBody] ToggleModulePermissionsRequest body, CancellationToken ct)
        {
            var actorId = GetActorId(User);
            if (string.IsNullOrWhiteSpace(module)) return BadRequest(new { message = "Module is required" });

            // Lấy tất cả permission keys thuộc module (Module.*)
            var prefix = module.Trim();
            var moduleKeys = await _db.Permissions.AsNoTracking()
                .Where(p => p.Name.StartsWith(prefix + ".", StringComparison.OrdinalIgnoreCase))
                .Select(p => p.Name)
                .ToListAsync(ct);

            if (moduleKeys.Count == 0)
                return NotFound(new { message = $"Module '{module}' không có quyền nào." });

            if (body?.Enable == true)
            {
                await _rolePerms.UpdateRolePermissionsDeltaByKeysAsync(roleId, moduleKeys, Array.Empty<string>(), actorId, ct);
                return Ok(new { updated = true, added = moduleKeys });
            }
            else
            {
                await _rolePerms.UpdateRolePermissionsDeltaByKeysAsync(roleId, Array.Empty<string>(), moduleKeys, actorId, ct);
                return Ok(new { updated = true, removed = moduleKeys });
            }
        }
    }
}
