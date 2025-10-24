using System;

namespace Patient.Domain.Entities;

public class PatientOwner
{
    public Guid PatientId { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public PatientEntity Patient { get; set; } = default!;
}
