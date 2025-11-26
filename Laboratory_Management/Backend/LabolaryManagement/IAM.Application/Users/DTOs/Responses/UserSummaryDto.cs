namespace IAM.Application.Users.DTOs.Responses;

/// <summary>
/// Summary information for user in list views
/// </summary>
public record UserSummaryDto
{
    public Guid UserId { get; init; }
    public string Username { get; init; }
    public string Email { get; init; }
    public string? FullName { get; init; }
    public bool IsActive { get; init; }
    public bool IsLocked { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? LastLoginAt { get; init; }
    public string[] Roles { get; init; }

    public UserSummaryDto(
        Guid userId,
        string username,
        string email,
        string? fullName,
        bool isActive,
        bool isLocked,
        DateTime createdAt,
        DateTime? lastLoginAt,
        string[] roles)
    {
        UserId = userId;
        Username = username;
        Email = email;
        FullName = fullName;
        IsActive = isActive;
        IsLocked = isLocked;
        CreatedAt = createdAt;
        LastLoginAt = lastLoginAt;
        Roles = roles;
    }
}
