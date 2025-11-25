namespace Patient.Application.Patients.DTOs.Responses;

public record PatientSummaryDto(
    Guid PatientId,
    string? FullName,
    DateOnly? DateOfBirth,
    byte Gender,
    string? BloodType,
    string? Phone,
    bool IsDeleted,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

