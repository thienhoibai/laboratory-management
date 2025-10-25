namespace Patient.Application.Messaging;

public interface IPatientEventPublisher
{
    Task PublishPatientCreatedAsync(Guid patientId, Guid actorUserId, string? nameNorm, DateTime createdAtUtc, CancellationToken ct = default);
    Task PublishPatientUpdatedAsync(Guid patientId, Guid actorUserId, DateTime updatedAtUtc, int changesCount, CancellationToken ct = default);
    Task PublishPatientDeletedAsync(Guid patientId, Guid actorUserId, DateTime deletedAtUtc, CancellationToken ct = default);
}
