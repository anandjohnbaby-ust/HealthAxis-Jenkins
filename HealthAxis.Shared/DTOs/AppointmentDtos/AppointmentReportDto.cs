namespace HealthAxis.Shared.DTOs.AdminDtos
{
    public class AppointmentReportDto
    {
        public DateOnly Date { get; set; }

        public int ConfirmedCount { get; set; }

        // Number of appointments that are pending (not yet confirmed/cancelled/completed)
        public int PendingCount { get; set; }

        public int CancelledCount { get; set; }

        public int CompletedCount { get; set; }

        public int TotalAppointments { get; set; }
    }
}