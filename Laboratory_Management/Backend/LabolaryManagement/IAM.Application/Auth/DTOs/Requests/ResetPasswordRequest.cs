namespace IAM.Application.Auth.DTOs.Requests;

/// <summary>
/// Request to reset password using token from email
/// </summary>
public record ResetPasswordRequest
{
    public required string Token { get; init; }
    public required string NewPassword { get; init; }
}

