namespace Contracts.Events.Patient;

// Minimal integration event for demo. Can be extended later.
public record PatientCreatedV1
(
    Guid PatientId,
    Guid RequestedByUserId,
    string Name,
    DateTime CreatedAtUtc
);
