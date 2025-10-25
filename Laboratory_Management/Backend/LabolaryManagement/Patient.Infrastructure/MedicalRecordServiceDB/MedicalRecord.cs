using System;
using System.Collections.Generic;

namespace Patient.Infrastructure.MedicalRecordServiceDB;

public partial class MedicalRecord
{
    public long RecordId { get; set; }

    public long PatientId { get; set; }

    public string? Diagnosis { get; set; }

    public string? Treatment { get; set; }

    public string? Notes { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? RecordName { get; set; }

    public virtual ICollection<TestHistory> TestHistories { get; set; } = new List<TestHistory>();
}
