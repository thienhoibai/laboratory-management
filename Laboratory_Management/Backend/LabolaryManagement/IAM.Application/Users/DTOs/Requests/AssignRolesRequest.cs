namespace IAM.Application.Users.DTOs.Requests;

/// <summary>
/// Request to assign roles to a user
/// </summary>
public record AssignRolesRequest
{
    public required int[] RoleIds { get; init; }
}
