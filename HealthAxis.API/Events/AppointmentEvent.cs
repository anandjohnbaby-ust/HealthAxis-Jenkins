namespace HealthAxis.API.Events
{
    public class AppointmentEvent
    {
        public string EventType { get; set; } = string.Empty;

        public int AppointmentId { get; set; }

        public int PatientId { get; set; }

        public int DoctorId { get; set; }

        public DateTime OccurredAt { get; set; }
    }
}
