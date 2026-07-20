using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace Messaging.Notifications
{
    public class RedisNotificationPublisher : INotificationPublisher
    {
        private readonly IConnectionMultiplexer _redis;
        private const string ChannelName = "lab.notify.v1";

        public RedisNotificationPublisher(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        public async Task PublishAsync(string eventName, object payload, CancellationToken ct = default)
        {
            var db = _redis.GetDatabase();
            var json = JsonSerializer.Serialize(payload);
            await db.PublishAsync(ChannelName, json);
        }
    }
}
