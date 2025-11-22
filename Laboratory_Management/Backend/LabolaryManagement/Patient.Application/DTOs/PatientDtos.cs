using System;

namespace Patient.Application.DTOs;

public record CreatePatientRequest(
    string FullName,
    DateOnly? DateOfBirth,
    byte Gender, // 0=Unknown,1=Male,2=Female,3=Other
    string? BloodType,
    string? Phone,
    string? Email,
    string? Address,
    string? CitizenId, // Đổi từ IdNumber → CitizenId
    string? InsuranceNumber,
    string? CreatedChannel
);

public record UpdatePatientRequest(
    string? FullName,
    DateOnly? DateOfBirth,
    byte? Gender,
    string? BloodType,
    string? Phone,
    string? Email,
    string? Address,
    string? CitizenId, // Đổi từ IdNumber → CitizenId
    string? InsuranceNumber
);

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

public record PatientDetailDto(
    Guid PatientId,
    string? FullName,
    DateOnly? DateOfBirth,
    byte Gender,
    string? BloodType,
    string? Phone,
    string? Email,
    string? Address,
    string? CitizenId, // Đổi từ IdNumber → CitizenId
    string? InsuranceNumber,
    Guid? LinkedUserId,
    bool IsDeleted,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public class PatientDto
{
    public Guid PatientId { get; set; }
    public string FullName { get; set; } = default!;
    public DateOnly? DateOfBirth { get; set; }
    public int Gender { get; set; }
    public string? BloodType { get; set; }
    public string Email { get; set; } = default!;
    public string Phone { get; set; } = default!;
    public string Address { get; set; } = default!;
    public string CitizenId { get; set; } // Đổi từ IdNumber → CitizenId
    public string? InsuranceNumber { get; set; }
    public DateTime CreatedAt { get; set; }
}
