namespace Patient.Application.Patients.DTOs.Responses;

public record PatientDetailDto(
    Guid PatientId,
    string? FullName,
    DateOnly? DateOfBirth,
    byte Gender,
    string? BloodType,
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

