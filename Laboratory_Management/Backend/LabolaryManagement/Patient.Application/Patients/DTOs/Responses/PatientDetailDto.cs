using Patient.Domain.Enums;

namespace Patient.Application.Patients.DTOs.Responses;

public record PatientDetailDto(
    Guid PatientId,
    string? FullName,
    DateOnly? DateOfBirth,
    byte Gender,
    BloodType BloodType, // Changed from string? to BloodType enum
    string? Phone,
    string? Email,
    string? Address,
    string? CitizenId,
    string? InsuranceNumber,
    Guid? LinkedUserId,
    bool IsDeleted,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

