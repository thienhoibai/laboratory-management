namespace IAM.Application.Auth.DTOs.Responses;

/// <summary>
/// Response after refreshing tokens
/// </summary>
public record RefreshResponse
{
    public string AccessToken { get; init; }
    public DateTime ExpiresAt { get; init; }
    public string RefreshToken { get; init; }

    public RefreshResponse(string accessToken, DateTime expiresAt, string refreshToken)
    {
        AccessToken = accessToken;
        ExpiresAt = expiresAt;
        RefreshToken = refreshToken;
    }
}

