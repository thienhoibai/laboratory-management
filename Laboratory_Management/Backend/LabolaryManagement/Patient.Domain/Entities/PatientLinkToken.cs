using System;

namespace Patient.Domain.Entities;

public class PatientLinkToken
{
    public Guid TokenId { get; set; }
    public Guid PatientId { get; set; }
    public byte[] TokenHash { get; set; } = Array.Empty<byte>();
    public DateTime ExpiresAt { get; set; }
    public DateTime? UsedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    // "magic" or "otp"
    public string Mode { get; set; } = "magic";
}
