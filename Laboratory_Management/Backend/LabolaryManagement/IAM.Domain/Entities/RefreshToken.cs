using System;

namespace IAM.Domain.Entities
{
    public class RefreshToken
    {
        public Guid RefreshTokenId { get; set; }
        public Guid UserId { get; set; }
        public string TokenHash { get; set; } = string.Empty;
        public DateTime IssuedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool Revoked { get; set; }
        public string? Device { get; set; }
        public string? IPAddress { get; set; }

        public User? User { get; set; }
    }
}
