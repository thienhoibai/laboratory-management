using Patient.Domain.Enums;

namespace Patient.Application.Patients.DTOs.Responses;

public record PatientSummaryDto(
    Guid PatientId,
    string? FullName,
    DateOnly? DateOfBirth,
    byte Gender,
    BloodType BloodType, // Changed from string? to BloodType enum
    string? email,
    string? Phone,
    bool IsDeleted,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

