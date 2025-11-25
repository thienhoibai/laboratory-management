namespace IAM.Application.Auth.DTOs.Requests;

/// <summary>
/// Request to logout and revoke refresh token
/// </summary>
public record LogoutRequest
{
    public required string RefreshToken { get; init; }
}

