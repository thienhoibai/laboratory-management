namespace IAM.Application.Users.DTOs.Responses;

/// <summary>
/// Detailed information for single user view
/// </summary>
public record UserDetailDto
{
    public Guid UserId { get; init; }
    public string Username { get; init; }
    public string Email { get; init; }
    public string? FullName { get; init; }
    public bool IsActive { get; init; }
    public bool IsLocked { get; init; }
    public DateTime? LastLoginAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public string[] Roles { get; init; }

    public UserDetailDto(
        Guid userId,
        string username,
        string email,
        string? fullName,
        bool isActive,
        bool isLocked,
        DateTime? lastLoginAt,
        DateTime createdAt,
        DateTime updatedAt,
        string[] roles)
    {
        UserId = userId;
        Username = username;
        Email = email;
        FullName = fullName;
        IsActive = isActive;
        IsLocked = isLocked;
        LastLoginAt = lastLoginAt;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        Roles = roles;
    }
}
