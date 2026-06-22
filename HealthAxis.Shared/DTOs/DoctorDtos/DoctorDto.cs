using HealthAxis.Shared.Enums;

namespace HealthAxis.Shared.DTOs.DoctorDtos
{
    public class DoctorDto
    {
        public int DoctorId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public Specialisation Specialisation { get; set; }

        public int YearsOfExperience { get; set; }

        public decimal ConsultationFee { get; set; }

        public bool IsActive { get; set; }

        public int UpcomingAppointmentCount { get; set; }
    }
}