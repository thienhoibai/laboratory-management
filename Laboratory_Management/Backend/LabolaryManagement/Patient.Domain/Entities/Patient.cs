using System;
using System.Collections.Generic;

namespace Patient.Domain.Entities;

public class PatientEntity
{
    public Guid PatientId { get; set; }

    // Plain PII stored as NVARCHAR
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? IdNumber { get; set; }
    public string? InsuranceNumber { get; set; }

    public byte Gender { get; set; }

    // Searchable columns
    public string? FullNameNorm { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? PhoneLast4 { get; set; }

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

    public ICollection<PatientEventLog> EventLogs { get; set; } = new List<PatientEventLog>();
    public ICollection<PatientRecordVersion> Versions { get; set; } = new List<PatientRecordVersion>();
}
