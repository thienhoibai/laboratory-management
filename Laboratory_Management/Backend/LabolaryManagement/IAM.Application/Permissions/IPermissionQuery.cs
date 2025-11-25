using IAM.Application.Permissions.DTOs.Responses;

namespace IAM.Application.Permissions
{
    public interface IPermissionQuery
    {
        Task<List<PermissionGroupDto>> GetGroupsAsync(CancellationToken ct = default);
    }
}
