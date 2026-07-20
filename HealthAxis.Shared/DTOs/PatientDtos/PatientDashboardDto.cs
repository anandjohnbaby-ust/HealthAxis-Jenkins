namespace HealthAxis.Shared.DTOs.PatientDtos
{
    public class PatientDashboardDto
    {
        public string FullName { get; set; } = string.Empty;

        public int TotalAppointments { get; set; }

        public int TotalHealthRecords { get; set; }

        public List<DashboardAppointmentDto> UpcomingAppointments { get; set; } = [];
    }
}