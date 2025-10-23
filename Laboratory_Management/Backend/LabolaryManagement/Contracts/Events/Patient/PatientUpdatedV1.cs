namespace Contracts.Events.Patient;

public record PatientUpdatedV1
(
    Guid PatientId,
    Guid RequestedByUserId,
    DateTime UpdatedAtUtc,
    int ChangesCount
);
