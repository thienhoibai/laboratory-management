using System;

namespace IAM.Domain.Entities
{
    public class PasswordResetToken
    {
        public Guid PasswordResetTokenId { get; set; }
        public Guid UserId { get; set; }
        public byte[] TokenHash { get; set; } = Array.Empty<byte>();
        public DateTime ExpiresAt { get; set; }
        public DateTime? UsedAt { get; set; }
        public string? RequestedIp { get; set; }
        public int Attempts { get; set; }
        public DateTime CreatedAt { get; set; }
        public User? User { get; set; }
    }
}
