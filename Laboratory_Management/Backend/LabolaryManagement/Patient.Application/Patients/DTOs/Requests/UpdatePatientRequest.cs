namespace Patient.Application.Patients.DTOs.Requests;

public record UpdatePatientRequest(
    string? FullName,
    DateOnly? DateOfBirth,
    byte? Gender,
    string? BloodType,
    string? Phone,
    string? Email,
    string? Address,
    string? CitizenId,
    string? InsuranceNumber
);

