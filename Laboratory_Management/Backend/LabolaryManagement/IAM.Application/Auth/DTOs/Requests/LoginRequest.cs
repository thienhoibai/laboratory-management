namespace IAM.Application.Auth.DTOs.Requests;

/// <summary>
/// Request for user login with username and password
/// </summary>
public record LoginRequest
{
    public required string Username { get; init; }
    public required string Password { get; init; }
}

