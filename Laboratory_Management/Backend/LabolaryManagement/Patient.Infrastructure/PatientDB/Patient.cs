using System;
using System.Collections.Generic;

namespace Patient.Infrastructure.PatientDB;

public partial class Patient
{
    public long PatientId { get; set; }

    public string FullName { get; set; } = null!;

    public DateOnly DateOfBirth { get; set; }

    public string? Gender { get; set; }

    public string? BloodGroup { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Address { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
