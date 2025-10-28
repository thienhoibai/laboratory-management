using System;

namespace Patient.Domain.Entities;

public class AuditLog
{
    public long AuditId { get; set; }
    public string Entity { get; set; } = "Patient";
    public Guid EntityId { get; set; }
    public string Action { get; set; } = string.Empty; // Create|Update|Delete
    public DateTime OccurredAt { get; set; }
    public Guid? UserId { get; set; }
    public string? CorrelationId { get; set; }
    public string? DetailJson { get; set; }
}
