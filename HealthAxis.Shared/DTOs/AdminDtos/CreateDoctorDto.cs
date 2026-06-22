using HealthAxis.Shared.Enums;
using System.ComponentModel.DataAnnotations;
using HealthAxis.Shared.Utilities;

namespace HealthAxis.Shared.DTOs.AdminDtos
{
    public class CreateDoctorDto
    {
        [Required(ErrorMessage = ValidationMessages.FullNameRequired)]
        [StringLength(ValidationLimits.FullNameLength)]
        [RegularExpression(RegexPatterns.FullName, ErrorMessage = ValidationMessages.InvalidFullNameFormat)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = ValidationMessages.SpecialisationRequired)]
        public Specialisation Specialisation { get; set; }

        [Range(ValidationLimits.MinExperience, ValidationLimits.MaxExperience, ErrorMessage = ValidationMessages.InvalidExperienceRange)]
        public int YearsOfExperience { get; set; }

        [Range(typeof(decimal), ValidationLimits.MinConsultationFee, ValidationLimits.MaxConsultationFee, ErrorMessage = ValidationMessages.InvalidConsultationFee)]
        public decimal ConsultationFee { get; set; }

        [Required(ErrorMessage = ValidationMessages.EmailRequired)]
        [EmailAddress(ErrorMessage = ValidationMessages.InvalidEmailFormat)]
        [StringLength(ValidationLimits.EmailLength)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = ValidationMessages.PasswordRequired)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
