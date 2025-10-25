using Microsoft.EntityFrameworkCore;

namespace Patient.Infrastructure.Outbox;

public static class OutboxSchemaInitializer
{
    public static async Task EnsureCreatedAsync(PatientDbContext db, CancellationToken ct = default)
    {
        var sql = @"
IF OBJECT_ID('outbox_messages', 'U') IS NULL
BEGIN
    CREATE TABLE outbox_messages(
      id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
      occurred_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
      message_type NVARCHAR(200) NOT NULL,
      payload_json NVARCHAR(MAX) NOT NULL,
      headers_json NVARCHAR(MAX) NULL,
      status TINYINT NOT NULL DEFAULT 0,
      retry_count INT NOT NULL DEFAULT 0,
      next_attempt_at DATETIME2 NULL,
      dedup_key NVARCHAR(200) NULL,
      correlation_id NVARCHAR(100) NULL,
      causation_id NVARCHAR(100) NULL
    );
END;

IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = 'IX_outbox_status_next')
    CREATE INDEX IX_outbox_status_next ON outbox_messages(status, next_attempt_at);

IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = 'IX_outbox_status_retry')
    CREATE INDEX IX_outbox_status_retry ON outbox_messages(status, retry_count);

IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = 'UX_outbox_dedup')
    CREATE UNIQUE INDEX UX_outbox_dedup ON outbox_messages(dedup_key) WHERE dedup_key IS NOT NULL;";
        await db.Database.ExecuteSqlRawAsync(sql, ct);
    }
}
