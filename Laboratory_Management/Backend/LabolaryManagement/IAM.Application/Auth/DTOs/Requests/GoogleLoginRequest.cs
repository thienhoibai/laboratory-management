namespace IAM.Application.Auth.DTOs.Requests;

/// <summary>
/// Request for Google OAuth login
/// </summary>
public record GoogleLoginRequest
{
    public required string IdToken { get; init; }
}

