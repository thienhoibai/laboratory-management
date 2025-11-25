namespace IAM.Application.Auth.DTOs.Requests;

/// <summary>
/// Request for user self-registration
/// </summary>
public record RegisterRequest
{
    public required string Username { get; init; }
    public required string Email { get; init; }
    public required string Password { get; init; }
    public required string ConfirmPassword { get; init; }
    public string? FullName { get; init; }
}

