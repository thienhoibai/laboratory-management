using System;

namespace Patient.Domain.Entities;

public class PatientOtpToken
{
    public Guid OtpId { get; set; }
    public Guid PatientId { get; set; }
    public byte[] CodeHash { get; set; } = Array.Empty<byte>();
    public DateTime ExpiresAt { get; set; }
    public int Attempts { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UsedAt { get; set; }
}
