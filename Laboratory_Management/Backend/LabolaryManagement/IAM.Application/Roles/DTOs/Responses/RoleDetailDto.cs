namespace IAM.Application.Roles.DTOs.Responses;

/// <summary>
/// Detailed information for single role view
/// </summary>
public record RoleDetailDto
{
    public int RoleId { get; init; }
    public string Name { get; init; }
    public string? Description { get; init; }
    public bool IsDefault { get; init; }
    public string[] Permissions { get; init; }

    public RoleDetailDto(int roleId, string name, string? description, bool isDefault, string[] permissions)
    {
        RoleId = roleId;
        Name = name;
        Description = description;
        IsDefault = isDefault;
        Permissions = permissions;
    }
}

