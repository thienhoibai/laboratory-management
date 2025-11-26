namespace IAM.Application.Users.DTOs.Requests;

/// <summary>
/// Request for admin to create a new user account
/// Admin provides username, email, and role. Password will be auto-generated and sent via email.
/// </summary>
public record CreateUserRequest
{
    public required string Username { get; init; }
    public required string Email { get; init; }
    public required int RoleId { get; init; }
}
