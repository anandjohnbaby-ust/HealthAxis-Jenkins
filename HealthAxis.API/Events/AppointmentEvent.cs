namespace HealthAxis.API.Events
{
    public class AppointmentEvent
    {
        public Guid EventId { get; set; } = Guid.NewGuid(); 

        public string EventType { get; set; } = string.Empty;

        public int AppointmentId { get; set; }

        public int PatientId { get; set; }

        public string PatientName { get; set; } = string.Empty;

        public int DoctorId { get; set; }

        public DateTime ScheduledDate { get; set; }

        public TimeOnly TimeSlot { get; set; }

        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    }
}