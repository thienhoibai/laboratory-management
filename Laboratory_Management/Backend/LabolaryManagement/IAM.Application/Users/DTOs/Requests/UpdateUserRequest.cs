namespace IAM.Application.Users.DTOs.Requests;

/// <summary>
/// Request to update user profile information
/// </summary>
public record UpdateUserRequest
{
    public string? Email { get; init; }
    public string? FullName { get; init; }
}
