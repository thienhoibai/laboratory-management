using System.Text.Json;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Messaging.Notifications
{
    public interface INotificationPublisher
    {
        Task PublishAsync(string eventName, object payload, CancellationToken ct = default);
    }

    public class LoggingNotificationPublisher : INotificationPublisher
    {
        private readonly ILogger<LoggingNotificationPublisher> _logger;
        public LoggingNotificationPublisher(ILogger<LoggingNotificationPublisher> logger)
        {
            _logger = logger;
        }

        public Task PublishAsync(string eventName, object payload, CancellationToken ct = default)
        {
            var json = JsonSerializer.Serialize(payload);
            _logger.LogInformation("[NotificationEvent] {Event} {Payload}", eventName, json);
            return Task.CompletedTask;
        }
    }
}
