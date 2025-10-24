using Contracts.Events.Patient;
using MassTransit;
using Patient.Application.Messaging;

namespace Patient.Presentation.Infrastructure;

public class MassTransitPatientEventPublisher : IPatientEventPublisher
{
    private readonly IPublishEndpoint _bus;
    public MassTransitPatientEventPublisher(IPublishEndpoint bus) => _bus = bus;

    public Task PublishPatientCreatedAsync(Guid patientId, Guid actorUserId, string? nameNorm, DateTime createdAtUtc, CancellationToken ct = default)
        => _bus.Publish(new PatientCreatedV1(patientId, actorUserId, nameNorm ?? string.Empty, createdAtUtc), ct);

    public Task PublishPatientUpdatedAsync(Guid patientId, Guid actorUserId, DateTime updatedAtUtc, int changesCount, CancellationToken ct = default)
        => _bus.Publish(new PatientUpdatedV1(patientId, actorUserId, updatedAtUtc, changesCount), ct);

    public Task PublishPatientDeletedAsync(Guid patientId, Guid actorUserId, DateTime deletedAtUtc, CancellationToken ct = default)
        => _bus.Publish(new PatientDeletedV1(patientId, actorUserId, deletedAtUtc), ct);
}
