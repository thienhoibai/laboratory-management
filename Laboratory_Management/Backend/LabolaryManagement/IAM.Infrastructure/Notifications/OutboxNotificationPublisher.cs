using System.Text.Json;
using System.Text.Json.Nodes;
using Contracts.Notifications;
using IAM.Infrastructure.Outbox;
using Messaging.Notifications;

namespace IAM.Infrastructure.Notifications;

public class OutboxNotificationPublisher : INotificationPublisher
{
    private readonly OutboxWriter _outbox;

    public OutboxNotificationPublisher(OutboxWriter outbox)
    {
        _outbox = outbox;
    }

    public Task PublishAsync(string eventName, object payload, CancellationToken ct = default)
    {
        // Map generic eventName/payload to NotificationRequestedV1 for email channel.
        // Expect payload contains recipient in 'to' or 'email'. Remaining fields go into Data.
        var node = JsonSerializer.SerializeToNode(payload) as JsonObject ?? new JsonObject();
        var to = node["to"]?.GetValue<string>() ?? node["email"]?.GetValue<string>();
        if (string.IsNullOrWhiteSpace(to)) throw new InvalidOperationException("Notification payload missing recipient (to/email)");

        // Build template from eventName
        var template = eventName switch
        {
            "PasswordResetRequested" => "ResetPassword",
            "VerifyEmailRequested" => "VerifyEmail",
            "InviteUser" => "InviteUser",
            _ => eventName
        };

        // Move all properties into data except recipient fields
        var data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var kv in node)
        {
            if (string.Equals(kv.Key, "to", StringComparison.OrdinalIgnoreCase) || string.Equals(kv.Key, "email", StringComparison.OrdinalIgnoreCase))
                continue;
            if (kv.Value is null) continue;
            data[kv.Key] = kv.Value!.ToJsonString();
        }

        var evt = new NotificationRequestedV1(
            MessageId: Guid.NewGuid().ToString(),
            Channel: "email",
            To: to!,
            Template: template,
            Data: data
        );

        return _outbox.AppendAsync(
            messageType: nameof(NotificationRequestedV1),
            payload: evt,
            dedupKey: evt.MessageId,
            correlationId: evt.CorrelationId,
            causationId: evt.CausationId,
            ct: ct);
    }
}
