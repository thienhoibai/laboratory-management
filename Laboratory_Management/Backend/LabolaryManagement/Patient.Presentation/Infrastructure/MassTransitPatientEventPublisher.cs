using Microsoft.Extensions.Logging;
using Patient.Application.Messaging;

namespace Patient.Presentation.Infrastructure;

public class MassTransitPatientEventPublisher : IPatientEventPublisher
{
    private readonly ILogger<MassTransitPatientEventPublisher> _logger;
    public MassTransitPatientEventPublisher(ILogger<MassTransitPatientEventPublisher> logger) => _logger = logger;

    public Task PublishPatientCreatedAsync(Guid patientId, Guid actorUserId, string? nameNorm, DateTime createdAtUtc, CancellationToken ct = default)
    {
        _logger.LogInformation("Patient created event: {PatientId} by {ActorUserId}", patientId, actorUserId);
        return Task.CompletedTask;
    }

    public Task PublishPatientUpdatedAsync(Guid patientId, Guid actorUserId, DateTime updatedAtUtc, int changesCount, CancellationToken ct = default)
    {
        _logger.LogInformation("Patient updated event: {PatientId} by {ActorUserId}", patientId, actorUserId);
        return Task.CompletedTask;
    }

    public Task PublishPatientDeletedAsync(Guid patientId, Guid actorUserId, DateTime deletedAtUtc, CancellationToken ct = default)
    {
        _logger.LogInformation("Patient deleted event: {PatientId} by {ActorUserId}", patientId, actorUserId);
        return Task.CompletedTask;
    }
}
