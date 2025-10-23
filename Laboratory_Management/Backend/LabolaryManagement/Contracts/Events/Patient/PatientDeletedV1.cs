namespace Contracts.Events.Patient;

public record PatientDeletedV1
(
    Guid PatientId,
    Guid RequestedByUserId,
    DateTime DeletedAtUtc
);
