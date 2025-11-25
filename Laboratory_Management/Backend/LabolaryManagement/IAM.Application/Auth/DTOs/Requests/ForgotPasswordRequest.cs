namespace IAM.Application.Auth.DTOs.Requests;

/// <summary>
/// Request to initiate password reset flow
/// </summary>
public record ForgotPasswordRequest
{
    public required string UsernameOrEmail { get; init; }
}

