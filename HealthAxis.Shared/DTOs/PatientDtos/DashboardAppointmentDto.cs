using HealthAxis.Shared.Enums;

namespace HealthAxis.Shared.DTOs.PatientDtos
{
    public class DashboardAppointmentDto
    {
        public int AppointmentId { get; set; }

        public string DoctorName { get; set; } = string.Empty;

        public DateTime ScheduledDate { get; set; }

        public TimeOnly TimeSlot { get; set; }

        public AppointmentStatus Status { get; set; }
    }
}