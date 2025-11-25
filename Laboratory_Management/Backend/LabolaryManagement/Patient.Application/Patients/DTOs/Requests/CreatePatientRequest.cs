namespace Patient.Application.Patients.DTOs.Requests;

public record CreatePatientRequest(
    string FullName,
    DateOnly? DateOfBirth,
    byte Gender, // 0=Unknown,1=Male,2=Female,3=Other
    string? BloodType,
    string? Phone,
    string? Email,
    string? Address,
    string? CitizenId,
    string? InsuranceNumber,
    string? CreatedChannel
);

