namespace IAM.Application.Auth.DTOs
{
    public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
    public record ForgotPasswordRequest(string UsernameOrEmail);
    public record ResetPasswordRequest(string Token, string NewPassword);
}
