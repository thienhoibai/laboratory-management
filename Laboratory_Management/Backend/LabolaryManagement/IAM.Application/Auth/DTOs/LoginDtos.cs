namespace IAM.Application.Auth.DTOs
{
    public record LoginRequest(string Username, string Password);

    public record LoginResponse(string AccessToken, DateTime ExpiresAt, string RefreshToken);

    public record RefreshRequest(string RefreshToken);

    public record RefreshResponse(string AccessToken, DateTime ExpiresAt, string RefreshToken);
}
