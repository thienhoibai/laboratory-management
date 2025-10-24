using Contracts.Notifications;
using Patient.Infrastructure.Outbox;

namespace Patient.Infrastructure.Notifications;

public class OutboxNotificationPublisher
{
    private readonly OutboxWriter _outbox;

    public OutboxNotificationPublisher(OutboxWriter outbox)
    {
        _outbox = outbox;
    }

    public Task PublishAsync(NotificationRequestedV1 evt, CancellationToken ct = default)
    {
        return _outbox.AppendAsync(
            messageType: nameof(NotificationRequestedV1),
            payload: evt,
            dedupKey: evt.MessageId,
            correlationId: evt.CorrelationId,
            causationId: evt.CausationId,
            ct: ct);
    }
}
