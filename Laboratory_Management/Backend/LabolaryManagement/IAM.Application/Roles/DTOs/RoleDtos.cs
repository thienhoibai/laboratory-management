namespace IAM.Application.Roles.DTOs
{
    public record CreateRoleRequest(string Name, string? Description, bool IsDefault = false);
    public record AssignPermissionsRequest(int[] PermissionIds);

    public record RoleSummaryDto(int RoleId, string Name, string? Description, bool IsDefault, string[] Permissions);
    public record RoleDetailDto(int RoleId, string Name, string? Description, bool IsDefault, string[] Permissions);
}
