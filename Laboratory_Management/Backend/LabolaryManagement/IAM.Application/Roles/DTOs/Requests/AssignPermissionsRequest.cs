namespace IAM.Application.Roles.DTOs.Requests;

/// <summary>
/// Request to assign permissions to a role
/// </summary>
public record AssignPermissionsRequest
{
    public required int[] PermissionIds { get; init; }
}

