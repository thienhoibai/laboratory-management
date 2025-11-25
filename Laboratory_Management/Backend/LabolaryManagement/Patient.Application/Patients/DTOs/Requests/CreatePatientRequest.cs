using Patient.Domain.Enums;

namespace Patient.Application.Patients.DTOs.Requests;

public record CreatePatientRequest(
    string FullName,
    DateOnly? DateOfBirth,
    byte Gender, // 0=Unknown,1=Male,2=Female,3=Other
    BloodType BloodType, // Changed from string? to BloodType enum (0=Unknown, 1=A+, 2=A-, 3=B+, 4=B-, 5=AB+, 6=AB-, 7=O+, 8=O-)
    string? Phone,
    string? Email,
    string? Address,
    string? CitizenId,
    string? InsuranceNumber,
    string? CreatedChannel
);

