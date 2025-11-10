using IAM.Application.Permissions.DTOs;

namespace IAM.Application.Permissions
{
    public interface IPermissionQuery
    {
        Task<List<PermissionGroupDto>> GetGroupsAsync(CancellationToken ct = default);
    }
}
