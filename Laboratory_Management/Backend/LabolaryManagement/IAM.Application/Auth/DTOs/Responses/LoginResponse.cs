namespace IAM.Application.Auth.DTOs.Responses;

/// <summary>
/// Response after successful login containing tokens
/// </summary>
public record LoginResponse
{
    public string AccessToken { get; init; }
    public DateTime ExpiresAt { get; init; }
    public string RefreshToken { get; init; }

    public LoginResponse(string accessToken, DateTime expiresAt, string refreshToken)
    {
        AccessToken = accessToken;
        ExpiresAt = expiresAt;
        RefreshToken = refreshToken;
    }
}

