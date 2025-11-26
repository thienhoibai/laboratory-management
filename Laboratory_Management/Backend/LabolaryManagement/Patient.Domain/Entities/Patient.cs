using System;
using Patient.Domain.Enums;

namespace Patient.Domain.Entities;

public class PatientEntity
{
    public Guid PatientId { get; set; }

    // Plain PII stored as NVARCHAR
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? CitizenId { get; set; }
    public string? InsuranceNumber { get; set; }

    public byte Gender { get; set; }
    public BloodType BloodType { get; set; } // Changed from string? to BloodType enum

    // Searchable columns (computed, không lưu trong DB mới)
    public string? FullNameNorm { get; set; }
    public DateOnly? DateOfBirth { get; set; }

    // Link to IAM user
    public Guid? UserId { get; set; }

    // Audit
    public string? CreatedChannel { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public Guid? UpdatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Soft delete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedByUserId { get; set; }
}
