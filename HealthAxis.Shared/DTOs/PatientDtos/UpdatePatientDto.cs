using HealthAxis.Shared.Enums;
using HealthAxis.Shared.Utilities;
using System.ComponentModel.DataAnnotations;

namespace HealthAxis.Shared.DTOs.PatientDtos
{
    public class UpdatePatientDto
    {
        [StringLength(ValidationLimits.FullNameLength)]
        [RegularExpression(
            RegexPatterns.FullName,
            ErrorMessage = ValidationMessages.InvalidFullNameFormat)]
        public string? FullName { get; set; }

        [DataType(DataType.Date)]
        [CustomValidation(
            typeof(PatientValidation),
            nameof(PatientValidation.ValidateDateOfBirth))]
        public DateTime? DateOfBirth { get; set; }

        public Gender? Gender { get; set; }

        [StringLength(ValidationLimits.PhoneNumberLength)]
        [RegularExpression(
            RegexPatterns.PhoneNumber,
            ErrorMessage = ValidationMessages.InvalidPhoneNumberFormat)]
        public string? PhoneNumber { get; set; }

        [EmailAddress(
            ErrorMessage = ValidationMessages.InvalidEmailFormat)]
        [StringLength(ValidationLimits.EmailLength)]
        public string? Email { get; set; }
    }


}   

