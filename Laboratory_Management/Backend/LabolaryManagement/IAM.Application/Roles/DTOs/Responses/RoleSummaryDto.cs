namespace IAM.Application.Roles.DTOs.Responses;

/// <summary>
/// Summary information for role in list views
/// </summary>
public record RoleSummaryDto
{
    public int RoleId { get; init; }
    public string Name { get; init; }
    public string? Description { get; init; }
    public bool IsDefault { get; init; }
    public string[] Permissions { get; init; }

    public RoleSummaryDto(int roleId, string name, string? description, bool isDefault, string[] permissions)
    {
        RoleId = roleId;
        Name = name;
        Description = description;
        IsDefault = isDefault;
        Permissions = permissions;
    }
}

