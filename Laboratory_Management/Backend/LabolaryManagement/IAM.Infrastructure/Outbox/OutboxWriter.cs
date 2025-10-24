using System.Text.Json;
using System.Text.Json.Serialization;
using IAM.Infrastructure.Outbox;

namespace IAM.Infrastructure.Outbox;

public class OutboxWriter
{
    private readonly IamDbContext _db;
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public OutboxWriter(IamDbContext db)
    {
        _db = db;
    }

    public Task AppendAsync(string messageType, object payload, string? dedupKey = null, string? correlationId = null, string? causationId = null, CancellationToken ct = default)
    {
        var msg = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            OccurredAt = DateTime.UtcNow,
            MessageType = messageType,
            PayloadJson = JsonSerializer.Serialize(payload, JsonOpts),
            HeadersJson = JsonSerializer.Serialize(new { correlationId, causationId }, JsonOpts),
            Status = 0,
            RetryCount = 0,
            NextAttemptAt = null,
            DedupKey = dedupKey,
            CorrelationId = correlationId,
            CausationId = causationId
        };
        _db.OutboxMessages.Add(msg);
        return Task.CompletedTask;
    }
}
