using System;

namespace IAM.Domain.Entities
{
    public class PasswordHistory
    {
        public Guid PasswordHistoryId { get; set; }
        public Guid UserId { get; set; }
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public User? User { get; set; }
    }
}
