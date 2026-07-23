using System.Threading;
using System.Threading.Tasks;

namespace Messaging.Notifications
{
    public interface INotificationPublisher
    {
        Task PublishAsync(string eventName, object payload, CancellationToken ct = default);
    }
}
