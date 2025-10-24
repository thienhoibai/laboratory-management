using System;

namespace Patient.Domain.Entities;

public class PatientEventLog
{
    public long LogId { get; set; }
    public Guid PatientId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public Guid? ActorUserId { get; set; }
    public string? Detail { get; set; }
    public DateTime OccurredAt { get; set; }
    public Guid? CorrelationId { get; set; }
    public string? TraceId { get; set; }
  
    public PatientEntity Patient { get; set; } = null!;
}
