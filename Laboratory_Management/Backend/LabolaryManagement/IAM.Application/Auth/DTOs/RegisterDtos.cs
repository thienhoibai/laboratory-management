namespace IAM.Application.Auth.DTOs
{
    public record RegisterRequest(string Username, string Email, string Password, string ConfirmPassword, string? FullName = null);
}
