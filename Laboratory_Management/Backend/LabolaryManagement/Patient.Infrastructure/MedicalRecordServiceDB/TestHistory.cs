using System;
using System.Collections.Generic;

namespace Patient.Infrastructure.MedicalRecordServiceDB;

public partial class TestHistory
{
    public long TestId { get; set; }

    public long RecordId { get; set; }

    public string TestType { get; set; } = null!;

    public DateTime? TestDate { get; set; }

    public string? Result { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual MedicalRecord Record { get; set; } = null!;
}
