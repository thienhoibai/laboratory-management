using Common.Errors;
using Common.Pagination;
using IAM.Application.Users.Services;
using IAM.Application.Users.DTOs.Requests;
using IAM.Application.Users.DTOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Common.Web.Filters;
using System.Security.Claims;

namespace IAM.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _users;
        public UsersController(IUserService users) => _users = users;

        private static Guid GetActorId(ClaimsPrincipal user)
        {
            var id = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? user.FindFirst("sub")?.Value;
            return id != null && Guid.TryParse(id, out var g) ? g : Guid.Empty;
        }

        [HttpGet]
        //[Authorize(Policy = "perm:User.List")]
        public async Task<ActionResult<PageResult<UserSummaryDto>>> List([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null,
            [FromQuery] string? role = null, [FromQuery] string? type = null, [FromQuery] string? status = null, [FromQuery] string? sortBy = null, [FromQuery] string? sort = null, CancellationToken ct = default)
        {
            var result = await _users.ListAsync(page, pageSize, search, role, type, status, sortBy, sort, ct);
            return Ok(result);
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<UserDetailDto>> Me(CancellationToken ct)
        {
            var actorId = GetActorId(User);
            if (actorId == Guid.Empty) throw new ApiException(ErrorCodes.Unauthorized);
            var res = await _users.GetAsync(actorId, actorId, allowOther: false, ct);
            if (!res.Succeeded) throw new ApiException(res.Error ?? ErrorCodes.NotFound);
            return Ok(res.Data);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<UserDetailDto>> Get(Guid id, CancellationToken ct)
        {
            var actorId = GetActorId(User);
            var res = await _users.GetAsync(id, actorId, allowOther: true, ct);
            if (!res.Succeeded) throw new ApiException(res.Error ?? ErrorCodes.NotFound);
            return Ok(res.Data);
        }

        [HttpPost]
        [Authorize(Policy = "perm:User.Create")]
        public async Task<ActionResult<UserDetailDto>> Create([FromBody] CreateUserRequest request, CancellationToken ct)
        {
            var actorId = GetActorId(User);
            var res = await _users.CreateAsync(request, actorId, ct);
            if (!res.Succeeded) throw new ApiException(res.Error ?? ErrorCodes.ValidationError);
            return CreatedAtAction(nameof(Get), new { id = res.Data!.UserId }, res.Data);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "perm:User.Update")]
        public async Task<ActionResult<UserDetailDto>> Update(Guid id, [FromBody] UpdateUserRequest request, CancellationToken ct)
        {
            var actorId = GetActorId(User);
            var res = await _users.UpdateAsync(id, request, actorId, ct);
            if (!res.Succeeded) throw new ApiException(res.Error ?? ErrorCodes.ValidationError);
            return Ok(res.Data);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "perm:User.Delete")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var actorId = GetActorId(User);
            var res = await _users.DeleteAsync(id, actorId, ct);
            if (!res.Succeeded) throw new ApiException(res.Error ?? ErrorCodes.Conflict);
            return NoContent();
        }

        [HttpPost("{id}/roles")]
        [Authorize(Policy = "perm:Role.Update")]
        public async Task<IActionResult> AssignRoles(Guid id, [FromBody] AssignRolesRequest request, CancellationToken ct)
        {
            var actorId = GetActorId(User);
            var res = await _users.AssignRolesAsync(id, request, actorId, ct);
            if (!res.Succeeded) throw new ApiException(res.Error ?? ErrorCodes.ValidationError);
            return Ok(new { userId = id, assignedRoleIds = request.RoleIds });
        }

        [HttpPost("{id}/lock")]
        [Authorize(Policy = "perm:User.Update")]
        public async Task<IActionResult> Lock(Guid id, CancellationToken ct)
        {
            var actorId = GetActorId(User);
            var res = await _users.LockAsync(id, actorId, ct);
            if (!res.Succeeded) throw new ApiException(res.Error ?? ErrorCodes.ValidationError);
            return Ok(new { userId = id, status = "locked" });
        }

        [HttpPost("{id}/unlock")]
        [Authorize(Policy = "perm:User.Update")]
        public async Task<IActionResult> Unlock(Guid id, CancellationToken ct)
        {
            var actorId = GetActorId(User);
            var res = await _users.UnlockAsync(id, actorId, ct);
            if (!res.Succeeded) throw new ApiException(res.Error ?? ErrorCodes.ValidationError);
            return Ok(new { userId = id, status = "unlocked" });
        }
    }
}
