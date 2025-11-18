using System;

namespace Patient.Domain.Entities;

public class PatientRecordVersion
{
    public long VersionId { get; set; }
    public Guid PatientId { get; set; }
    public int VersionNo { get; set; }
    public Guid ChangedBy { get; set; }
    public DateTime ChangedAt { get; set; }
    public string? ChangeSet { get; set; }
    public string? FullSnapshot { get; set; }

    public PatientEntity Patient { get; set; } = null!;
}
