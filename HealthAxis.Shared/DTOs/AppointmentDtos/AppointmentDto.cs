using HealthAxis.Shared.Enums;

namespace HealthAxis.Shared.DTOs.AppointmentDtos
{
    public class AppointmentDto
    {
        public int AppointmentId { get; set; }

        public int PatientId { get; set; }

        public string PatientName { get; set; } = string.Empty;

        public int DoctorId { get; set; }

        public string DoctorName { get; set; } = string.Empty;

        public DateTime ScheduledDate { get; set; }

        public TimeOnly TimeSlot { get; set; } 

        public AppointmentStatus Status { get; set; }

        public string? CancellationReason { get; set; }

        // ADD THIS
        public int? HealthRecordId { get; set; }
    }
}