using Common.Results;
using IAM.Application.Roles.DTOs.Requests;
using IAM.Application.Roles.DTOs.Responses;

namespace IAM.Application.Roles.Services;

public interface IRoleService
{
    Task<OperationResult<RoleDetailDto>> CreateAsync(CreateRoleRequest request, Guid actorId, CancellationToken ct = default);
    Task<OperationResult> DeleteAsync(int roleId, Guid actorId, CancellationToken ct = default);
    Task<OperationResult> AssignPermissionsAsync(int roleId, AssignPermissionsRequest request, Guid actorId, CancellationToken ct = default);
}
