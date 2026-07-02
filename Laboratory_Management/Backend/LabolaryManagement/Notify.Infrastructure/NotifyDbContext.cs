using Microsoft.EntityFrameworkCore;

namespace Notify.Infrastructure;

public class NotifyDbContext : DbContext
{
    public NotifyDbContext(DbContextOptions<NotifyDbContext> options) : base(options) { }

    public DbSet<NotificationJob> Jobs => Set<NotificationJob>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NotificationJob>(b =>
        {
            b.ToTable("notification_jobs");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            b.Property(x => x.MessageId).HasColumnName("message_id").HasMaxLength(100).IsRequired();
            b.Property(x => x.Channel).HasColumnName("channel").HasMaxLength(20).IsRequired();
            b.Property(x => x.Recipient).HasColumnName("recipient").HasMaxLength(320).IsRequired();
            b.Property(x => x.Template).HasColumnName("template").HasMaxLength(100).IsRequired();
            b.Property(x => x.DataJson).HasColumnName("data_json").IsRequired();
            b.Property(x => x.Status).HasColumnName("status").HasDefaultValue(0);
            b.Property(x => x.Error).HasColumnName("error").HasMaxLength(1000);
            b.Property(x => x.QueuedAt).HasColumnName("queued_at").HasDefaultValueSql("now()");
            b.Property(x => x.SentAt).HasColumnName("sent_at");
            b.Property(x => x.ProviderMessageId).HasColumnName("provider_message_id").HasMaxLength(200);
            b.HasIndex(x => new { x.Status, x.QueuedAt }).HasDatabaseName("IX_jobs_status");
            b.HasIndex(x => x.MessageId).IsUnique();
        });
    }
}

public class NotificationJob
{
    public Guid Id { get; set; }
    public string MessageId { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty; // email/sms/push
    public string Recipient { get; set; } = string.Empty;
    public string Template { get; set; } = string.Empty;
    public string DataJson { get; set; } = string.Empty;
    public byte Status { get; set; } // 0=queued,1=sending,2=sent,3=failed,4=dead
    public string? Error { get; set; }
    public DateTime QueuedAt { get; set; }
    public DateTime? SentAt { get; set; }
    public string? ProviderMessageId { get; set; }
}
