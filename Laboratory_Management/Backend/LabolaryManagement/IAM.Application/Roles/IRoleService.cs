using Common.Results;
using IAM.Application.Roles.DTOs;

namespace IAM.Application.Roles
{
    public interface IRoleService
    {
        Task<OperationResult<RoleDetailDto>> CreateAsync(CreateRoleRequest request, Guid actorId, CancellationToken ct = default);
        Task<OperationResult> DeleteAsync(int roleId, Guid actorId, CancellationToken ct = default);
        Task<OperationResult> AssignPermissionsAsync(int roleId, AssignPermissionsRequest request, Guid actorId, CancellationToken ct = default);
    }
}
