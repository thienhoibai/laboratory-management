using Patient.Domain.Enums;

namespace Patient.Application.Patients.DTOs.Responses;

public record PatientSummaryDto(
    Guid PatientId,
    string? FullName,
    DateOnly? DateOfBirth,
    byte Gender,
    BloodType BloodType,
    string? Email,
    string? Phone,
    bool IsDeleted,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

