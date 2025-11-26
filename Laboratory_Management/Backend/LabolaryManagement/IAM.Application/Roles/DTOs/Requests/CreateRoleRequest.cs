namespace IAM.Application.Roles.DTOs.Requests;

/// <summary>
/// Request to create a new role
/// </summary>
public record CreateRoleRequest
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public bool IsDefault { get; init; } = false;
}

