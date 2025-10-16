using System;

namespace IAM.Domain.Entities
{
    public class AuditLog
    {
        public Guid AuditLogId { get; set; }
        public Guid? UserId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string Resource { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ActorIp { get; set; }
        public DateTime CreatedAt { get; set; }

        public User? User { get; set; }
    }
}
