namespace Patient.Application.Patients.DTOs.Responses;

public class PatientDto
{
    public Guid PatientId { get; set; }
    public string FullName { get; set; } = default!;
    public DateOnly? DateOfBirth { get; set; }
    public int Gender { get; set; }
    public string? BloodType { get; set; }
    public string Email { get; set; } = default!;
    public string Phone { get; set; } = default!;
    public string Address { get; set; } = default!;
    public string CitizenId { get; set; } = default!;
    public string? InsuranceNumber { get; set; }
    public DateTime CreatedAt { get; set; }
}

