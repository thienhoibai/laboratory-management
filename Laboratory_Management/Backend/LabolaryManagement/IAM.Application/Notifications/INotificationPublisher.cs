using Contracts.Notifications;

namespace IAM.Application.Notifications;

public interface INotificationPublisher
{
    Task PublishAsync(NotificationRequestedV1 evt, CancellationToken ct = default);
}
