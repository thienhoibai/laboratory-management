using System.Text.Json;
using System.Text.Json.Serialization;
using Contracts.Notifications;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IAM.Infrastructure.Outbox;

public class OutboxProcessor : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxProcessor> _logger;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public OutboxProcessor(IServiceScopeFactory scopeFactory, ILogger<OutboxProcessor> logger)
    {
        _scopeFactory = scopeFactory; _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            cts.CancelAfter(TimeSpan.FromSeconds(12)); // avoid long-hanging DB ops
            try { await ProcessBatchAsync(cts.Token); }
            catch (OperationCanceledException) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogWarning("OutboxProcessor tick canceled due to timeout");
            }
            catch (Exception ex) { _logger.LogError(ex, "OutboxProcessor error"); }
        }
    }

    private async Task ProcessBatchAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IamDbContext>();
        var bus = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        // Fail fast if DB not reachable
        using var pingCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        pingCts.CancelAfter(TimeSpan.FromSeconds(5));
        if (!await db.Database.CanConnectAsync(pingCts.Token))
        {
            _logger.LogWarning("OutboxProcessor cannot connect to DB; will retry next tick");
            return;
        }

        db.Database.SetCommandTimeout(TimeSpan.FromSeconds(10));

        var now = DateTime.UtcNow;
        var batch = await db.OutboxMessages
            .Where(m => m.Status == 0 && (m.NextAttemptAt == null || m.NextAttemptAt <= now))
            .OrderBy(m => m.OccurredAt)
            .Take(50)
            .ToListAsync(ct);

        foreach (var m in batch)
        {
            m.Status = 3; // inflight
        }
        await db.SaveChangesAsync(ct);

        foreach (var m in batch)
        {
            try
            {
                if (m.MessageType == nameof(NotificationRequestedV1))
                {
                    var evt = JsonSerializer.Deserialize<NotificationRequestedV1>(m.PayloadJson, JsonOpts)!;
                    await bus.Publish(evt, ctx => { ctx.SetRoutingKey(evt.Channel); }, ct);
                    m.Status = 1; // sent
                }
                else
                {
                    _logger.LogWarning("Unknown outbox message type: {Type}", m.MessageType);
                    m.Status = 1; // mark as sent to skip
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Publishing canceled due to timeout for {Id}", m.Id);
                m.Status = 2; m.RetryCount++; m.NextAttemptAt = DateTime.UtcNow.AddSeconds(30);
            }
            catch (Exception ex)
            {
                m.Status = 2; // failed
                m.RetryCount++;
                var delay = TimeSpan.FromSeconds(Math.Min(300, 5 * Math.Pow(2, m.RetryCount)));
                m.NextAttemptAt = DateTime.UtcNow.Add(delay);
                _logger.LogError(ex, "Outbox publish failed for {Id}", m.Id);
            }
        }

        await db.SaveChangesAsync(ct);
    }
}
