using Contracts.Notifications;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Messaging.Email;
using Notify.Infrastructure;
using System.Text.Json;

namespace Notify.App.Consumers;

public class NotificationRequestedConsumer : IConsumer<NotificationRequestedV1>
{
    private readonly NotifyDbContext _db;
    private readonly IEmailSender _email;
    private readonly IEmailTemplateRenderer _renderer;
    private readonly ILogger<NotificationRequestedConsumer> _logger;

    public NotificationRequestedConsumer(NotifyDbContext db, IEmailSender email, IEmailTemplateRenderer renderer, ILogger<NotificationRequestedConsumer> logger)
    {
        _db = db; _email = email; _renderer = renderer; _logger = logger;
    }

    public async Task Consume(ConsumeContext<NotificationRequestedV1> context)
    {
        var evt = context.Message;
        // Idempotency
        var existing = await _db.Jobs.AsNoTracking().FirstOrDefaultAsync(j => j.MessageId == evt.MessageId, context.CancellationToken);
        if (existing != null && existing.Status == 2)
        {
            // Already processed (sent)
            _logger.LogInformation("Skip duplicate message {MessageId}", evt.MessageId);
            return;
        }

        var job = existing ?? new NotificationJob
        {
            Id = Guid.NewGuid(),
            MessageId = evt.MessageId,
            Channel = evt.Channel,
            Recipient = evt.To,
            Template = evt.Template,
            DataJson = JsonSerializer.Serialize(evt.Data)
        };

        if (existing == null)
        {
            _db.Jobs.Add(job);
            await _db.SaveChangesAsync(context.CancellationToken);
        }

        try
        {
            job.Status = 1; // sending
            await _db.SaveChangesAsync(context.CancellationToken);

            if (evt.Channel == "email")
            {
                var html = await _renderer.RenderAsync(evt.Template, evt.Data, context.CancellationToken);
                await _email.SendAsync(evt.To, evt.Template, html, context.CancellationToken);
            }
            else
            {
                _logger.LogWarning("Unsupported channel {Channel}", evt.Channel);
            }

            job.Status = 2; // sent
            job.SentAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(context.CancellationToken);
        }
        catch (Exception ex)
        {
            job.Status = 3; // failed
            job.Error = ex.Message;
            await _db.SaveChangesAsync(context.CancellationToken);
            throw; // allow MT to retry
        }
    }
}
