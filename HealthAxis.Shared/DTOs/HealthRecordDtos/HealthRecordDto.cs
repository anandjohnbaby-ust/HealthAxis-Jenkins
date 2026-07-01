namespace HealthAxis.Shared.DTOs.HealthRecordDtos
{
    public class HealthRecordDto
    {
        public int RecordId { get; set; }

        public int AppointmentId { get; set; }

        public int DoctorId { get; set; }

        public string DoctorName { get; set; } = string.Empty;

        public int PatientId { get; set; }

        public DateTime VisitDate { get; set; }

        public string Diagnosis { get; set; } = string.Empty;

        public string Prescription { get; set; } = string.Empty;

        public string Notes { get; set; } = string.Empty;
    }
}
