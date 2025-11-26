namespace IAM.Application.Roles.Services;

public interface IRolePermissionService
{
    Task<List<string>> GetPermissionKeysByRoleAsync(int roleId, CancellationToken ct = default);
    Task UpdateRolePermissionsByKeysAsync(int roleId, IEnumerable<string> keys, Guid actorId, CancellationToken ct = default);
    Task UpdateRolePermissionsDeltaByKeysAsync(int roleId, IEnumerable<string> addKeys, IEnumerable<string> removeKeys, Guid actorId, CancellationToken ct = default);
}
