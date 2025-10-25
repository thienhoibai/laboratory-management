namespace Patient.Infrastructure.Outbox;

public class OutboxMessage
{
    public Guid Id { get; set; }
    public DateTime OccurredAt { get; set; }
    public string MessageType { get; set; } = string.Empty;
    public string PayloadJson { get; set; } = string.Empty;
    public string? HeadersJson { get; set; }
    public byte Status { get; set; } // 0=pending,1=sent,2=failed,3=inflight
    public int RetryCount { get; set; }
    public DateTime? NextAttemptAt { get; set; }
    public string? DedupKey { get; set; }
    public string? CorrelationId { get; set; }
    public string? CausationId { get; set; }
}
