using System.Text.Json;
using Contracts.Notifications;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Notify.App.Consumers;
using StackExchange.Redis;

namespace Notify.Api.Services;

public class RedisNotificationSubscriberService : BackgroundService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RedisNotificationSubscriberService> _logger;
    private const string ChannelName = "lab.notify.v1";

    public RedisNotificationSubscriberService(
        IConnectionMultiplexer redis,
        IServiceProvider serviceProvider,
        ILogger<RedisNotificationSubscriberService> logger)
    {
        _redis = redis;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("RedisNotificationSubscriberService started. Listening on channel: {Channel}", ChannelName);

        var subscriber = _redis.GetSubscriber();

        await subscriber.SubscribeAsync(RedisChannel.Literal(ChannelName), async (channel, value) =>
        {
            if (value.IsNullOrEmpty) return;

            try
            {
                var evt = JsonSerializer.Deserialize<NotificationRequestedV1>(value.ToString());
                if (evt == null)
                {
                    _logger.LogWarning("Received empty or invalid notification message.");
                    return;
                }

                _logger.LogInformation("Received notification requested event: {MessageId} for {To}", evt.MessageId, evt.To);
                await ProcessWithRetryAsync(evt, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deserializing or processing notification message.");
            }
        });

        // Keep service alive until cancellation is requested
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }

        _logger.LogInformation("RedisNotificationSubscriberService stopping.");
        await subscriber.UnsubscribeAsync(RedisChannel.Literal(ChannelName));
    }

    private async Task ProcessWithRetryAsync(NotificationRequestedV1 evt, CancellationToken ct)
    {
        int maxRetries = 3;
        var retryDelays = new[] { TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(30) };

        for (int attempt = 1; attempt <= maxRetries + 1; attempt++)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var consumer = scope.ServiceProvider.GetRequiredService<NotificationRequestedConsumer>();
                await consumer.ConsumeAsync(evt, ct);
                return; // Success
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message on attempt {Attempt}/{MaxAttempts}", attempt, maxRetries + 1);
                if (attempt > maxRetries)
                {
                    _logger.LogCritical("Message {MessageId} failed after all retry attempts.", evt.MessageId);
                    throw;
                }
                var delay = retryDelays[attempt - 1];
                _logger.LogInformation("Retrying in {Delay}...", delay);
                await Task.Delay(delay, ct);
            }
        }
    }
}
