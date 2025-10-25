namespace Contracts.Notifications;

public record NotificationRequestedV1(
    string MessageId,
    string Channel,
    string To,
    string Template,
    IDictionary<string, string> Data,
    string? CorrelationId = null,
    string? CausationId = null,
    DateTimeOffset? ScheduledAt = null
);

public record NotificationSentV1(string MessageId, DateTimeOffset SentAt);
public record NotificationFailedV1(string MessageId, string Error, int RetryCount);
