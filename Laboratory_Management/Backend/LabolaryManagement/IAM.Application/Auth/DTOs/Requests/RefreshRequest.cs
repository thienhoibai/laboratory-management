namespace IAM.Application.Auth.DTOs.Requests;

/// <summary>
/// Request to refresh access token using refresh token
/// </summary>
public record RefreshRequest
{
    public required string RefreshToken { get; init; }
}

