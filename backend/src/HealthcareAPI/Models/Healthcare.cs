namespace HealthcareAPI.Models;

public class Patient
{
    public int Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? BloodType { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? EmergencyContact { get; set; }
    public ICollection<Appointment> Appointments { get; set; } = [];
    public ICollection<MedicalRecord> MedicalRecords { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Doctor
{
    public int Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string Specialization { get; set; }
    public string? License { get; set; }
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }
    public decimal ConsultationFee { get; set; }
    public double Rating { get; set; } = 5.0;
    public ICollection<Appointment> Appointments { get; set; } = [];
}

public class Appointment
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public Patient? Patient { get; set; }
    public int DoctorId { get; set; }
    public Doctor? Doctor { get; set; }
    public DateTime ScheduledAt { get; set; }
    public int DurationMinutes { get; set; } = 30;
    public string Status { get; set; } = "scheduled"; // scheduled|confirmed|in_progress|completed|cancelled
    public string Type { get; set; } = "in-person"; // in-person|video|phone
    public string? Notes { get; set; }
    public string? VideoLink { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class MedicalRecord
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public Patient? Patient { get; set; }
    public int DoctorId { get; set; }
    public required string Diagnosis { get; set; }
    public string? Treatment { get; set; }
    public string? Notes { get; set; }
    public int? AppointmentId { get; set; }
    public ICollection<Prescription> Prescriptions { get; set; } = [];
    public ICollection<LabResult> LabResults { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Prescription
{
    public int Id { get; set; }
    public int MedicalRecordId { get; set; }
    public required string Medication { get; set; }
    public required string Dosage { get; set; }
    public required string Frequency { get; set; }
    public int DurationDays { get; set; }
    public string? Instructions { get; set; }
}

public class LabResult
{
    public int Id { get; set; }
    public int MedicalRecordId { get; set; }
    public required string TestName { get; set; }
    public required string Result { get; set; }
    public string? Unit { get; set; }
    public string? ReferenceRange { get; set; }
    public string Status { get; set; } = "normal"; // normal|high|low|critical
    public DateTime TestedAt { get; set; } = DateTime.UtcNow;
}
