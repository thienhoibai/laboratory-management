using Microsoft.EntityFrameworkCore;

namespace IAM.Infrastructure.Outbox;

public static class OutboxSchemaInitializer
{
    public static async Task EnsureCreatedAsync(IamDbContext db, CancellationToken ct = default)
    {
        var sql = @"
CREATE TABLE IF NOT EXISTS outbox_messages(
  id UUID NOT NULL PRIMARY KEY,
  occurred_at TIMESTAMP NOT NULL DEFAULT now(),
  message_type VARCHAR(200) NOT NULL,
  payload_json TEXT NOT NULL,
  headers_json TEXT NULL,
  status SMALLINT NOT NULL DEFAULT 0,
  retry_count INT NOT NULL DEFAULT 0,
  next_attempt_at TIMESTAMP NULL,
  dedup_key VARCHAR(200) NULL,
  correlation_id VARCHAR(100) NULL,
  causation_id VARCHAR(100) NULL
);

CREATE INDEX IF NOT EXISTS IX_outbox_status_next ON outbox_messages(status, next_attempt_at);
CREATE INDEX IF NOT EXISTS IX_outbox_status_retry ON outbox_messages(status, retry_count);
CREATE UNIQUE INDEX IF NOT EXISTS UX_outbox_dedup ON outbox_messages(dedup_key) WHERE dedup_key IS NOT NULL;";
        await db.Database.ExecuteSqlRawAsync(sql, ct);
    }
}
