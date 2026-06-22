using HealthAxis.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace HealthAxis.Shared.DTOs.AdminDtos
{
    public class UpdateDoctorDto
    {
        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public Specialisation Specialisation { get; set; }

        [Range(0, 50)]
        public int YearsOfExperience { get; set; }

        [Range(typeof(decimal), "0", "999999")]
        public decimal ConsultationFee { get; set; }

        public bool IsActive { get; set; }
    }

}
