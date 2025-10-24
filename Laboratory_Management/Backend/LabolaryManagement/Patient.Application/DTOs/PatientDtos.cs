using System;

namespace Patient.Application.DTOs;

public record CreatePatientRequest(
    string FullName,
    DateOnly? DateOfBirth,
    byte Gender, // 0=Unknown,1=Male,2=Female,3=Other
    string? Phone,
    string? Email,
    string? Address,
    string? IdNumber,
    string? InsuranceNumber,
    Guid? UserId,
    string? CreatedChannel
);

public record UpdatePatientRequest(
    string? FullName,
    DateOnly? DateOfBirth,
    byte? Gender,
    string? Phone,
    string? Email,
    string? Address,
    string? IdNumber,
    string? InsuranceNumber,
    Guid? UserId
);

public record PatientSummaryDto(
    Guid PatientId,
    string? FullName,
    DateOnly? DateOfBirth,
    byte Gender,
    string? PhoneLast4,
    bool IsDeleted,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record PatientDetailDto(
    Guid PatientId,
    string? FullName,
    DateOnly? DateOfBirth,
    byte Gender,
    string? Phone,
    string? Email,
    string? Address,
    string? IdNumber,
    string? InsuranceNumber,
    Guid? LinkedUserId,
    bool IsDeleted,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record PatientVersionDto(
    long VersionId,
    int VersionNo,
    Guid ChangedBy,
    DateTime ChangedAt,
    string? ChangeSet,
    string? FullSnapshot
);
