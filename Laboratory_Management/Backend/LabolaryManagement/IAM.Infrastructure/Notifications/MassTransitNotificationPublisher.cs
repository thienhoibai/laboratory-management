using System.Text.Json;
using System.Text.Json.Nodes;
using Contracts.Notifications;
using MassTransit;
using Messaging.Notifications;

namespace IAM.Infrastructure.Notifications;

public class MassTransitNotificationPublisher : INotificationPublisher
{
    private readonly IPublishEndpoint _bus;

    public MassTransitNotificationPublisher(IPublishEndpoint bus)
    {
        _bus = bus;
    }

    public Task PublishAsync(string eventName, object payload, CancellationToken ct = default)
    {
        var node = JsonSerializer.SerializeToNode(payload) as JsonObject ?? new JsonObject();
        var to = node["to"]?.GetValue<string>() ?? node["email"]?.GetValue<string>();
        if (string.IsNullOrWhiteSpace(to)) throw new InvalidOperationException("Notification payload missing recipient (to/email)");

        var template = eventName switch
        {
            "PasswordResetRequested" => "ResetPassword",
            "VerifyEmailRequested" => "VerifyEmail",
            "InviteUser" => "InviteUser",
            _ => eventName
        };

        var data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var kv in node)
        {
            if (string.Equals(kv.Key, "to", StringComparison.OrdinalIgnoreCase) || string.Equals(kv.Key, "email", StringComparison.OrdinalIgnoreCase))
                continue;
            if (kv.Value is null) continue;

            if (kv.Value is JsonValue jv)
            {
                try { data[kv.Key] = jv.GetValue<string>(); }
                catch { data[kv.Key] = jv.ToJsonString(); }
            }
            else
            {
                data[kv.Key] = kv.Value.ToJsonString();
            }
        }

        var evt = new NotificationRequestedV1(
            MessageId: Guid.NewGuid().ToString(),
            Channel: "email",
            To: to!,
            Template: template,
            Data: data
        );

        // Routing key = channel ("email"). Exchange is configured in MassTransit.
        return _bus.Publish(evt, ctx => { ctx.SetRoutingKey(evt.Channel); }, ct);
    }
}
