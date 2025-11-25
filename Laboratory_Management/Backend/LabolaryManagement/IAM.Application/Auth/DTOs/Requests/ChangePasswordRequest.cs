namespace IAM.Application.Auth.DTOs.Requests;

/// <summary>
/// Request to change password for authenticated user
/// </summary>
public record ChangePasswordRequest
{
    public required string CurrentPassword { get; init; }
    public required string NewPassword { get; init; }
}

