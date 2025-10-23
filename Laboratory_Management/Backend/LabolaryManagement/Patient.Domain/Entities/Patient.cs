using System;
using System.Collections.Generic;

namespace Patient.Domain.Entities;

public class PatientEntity
{
    public Guid PatientId { get; set; }

    // Encrypted PII (VARBINARY)
    public byte[]? FullNameEnc { get; set; }
    public byte[]? DobEnc { get; set; }
    public byte[]? PhoneEnc { get; set; }
    public byte[]? EmailEnc { get; set; }
    public byte[]? AddressEnc { get; set; }
    public byte[]? IdNumberEnc { get; set; }
    public byte[]? InsuranceNumberEnc { get; set; }

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

    // FIX: instantiate concrete collection types instead of the interface
    public ICollection<PatientEventLog> EventLogs { get; set; } = new List<PatientEventLog>();
    public ICollection<PatientRecordVersion> Versions { get; set; } = new List<PatientRecordVersion>();
}
