using Common.Errors;
using Common.Web.Filters;
using IAM.Application.Roles.Services;
using IAM.Application.Roles.DTOs.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IAM.Presentation.Controllers
{
    [ApiController]
    [Route("api/roles")]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _roles;
        public RolesController(IRoleService roles) => _roles = roles;

        private static Guid GetActorId(ClaimsPrincipal user)
        {
            var id = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? user.FindFirst("sub")?.Value;
            return id != null && Guid.TryParse(id, out var g) ? g : Guid.Empty;
        }

        [HttpPost]
        [Authorize(Policy = "perm:Role.Create")]
        public async Task<IActionResult> Create([FromBody] CreateRoleRequest request, CancellationToken ct)
        {
            var actorId = GetActorId(User);
            var res = await _roles.CreateAsync(request, actorId, ct);
            if (!res.Succeeded) throw new ApiException(res.Error ?? ErrorCodes.ValidationError);
            return CreatedAtAction(nameof(Create), new { id = res.Data!.RoleId }, res.Data);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Policy = "perm:Role.Delete")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var actorId = GetActorId(User);
            var res = await _roles.DeleteAsync(id, actorId, ct);
            if (!res.Succeeded) throw new ApiException(res.Error ?? ErrorCodes.ValidationError);
            return NoContent();
        }

        [HttpPost("{id:int}/permissions")]
        [Authorize(Policy = "perm:Role.Update")]
        public async Task<IActionResult> AssignPermissions(int id, [FromBody] AssignPermissionsRequest request, CancellationToken ct)
        {
            var actorId = GetActorId(User);
            var res = await _roles.AssignPermissionsAsync(id, request, actorId, ct);
            if (!res.Succeeded) throw new ApiException(res.Error ?? ErrorCodes.ValidationError);
            return Ok(new { roleId = id, assignedPermissionIds = request.PermissionIds });
        }
    }
}
