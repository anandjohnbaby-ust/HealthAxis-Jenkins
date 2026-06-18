using HealthAxis.API.Enums;

namespace HealthAxis.API.DTOs.AdminDtos
{
    public class CreateDoctorDto
    {
        public string FullName { get; set; } = string.Empty;

        public Specialisation Specialisation { get; set; }

        public int YearsOfExperience { get; set; }

        public decimal ConsultationFee { get; set; }

        public bool IsActive { get; set; }
    }
}