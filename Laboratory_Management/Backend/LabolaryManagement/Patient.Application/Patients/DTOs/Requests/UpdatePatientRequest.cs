using Patient.Domain.Enums;

namespace Patient.Application.Patients.DTOs.Requests;

public record UpdatePatientRequest(
    string? FullName,
    DateOnly? DateOfBirth,
    byte? Gender,
    BloodType? BloodType, // Changed from string? to BloodType? enum (nullable for partial updates)
    string? Phone,
    string? Email,
    string? Address,
    string? CitizenId,
    string? InsuranceNumber
);

