using HealthAxis.API.Enums;
using HealthAxis.API.Utilities;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthAxis.API.Models
{
    public class Patient
    {
        [Key]
        public int PatientId { get; set; }

        [Required(ErrorMessage = ValidationMessages.FullNameRequired)]
        [StringLength(ValidationLimits.FullNameLength)]
        [RegularExpression(RegexPatterns.FullName, ErrorMessage = ValidationMessages.InvalidFullNameFormat)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = ValidationMessages.DateOfBirthRequired)]
        [DataType(DataType.Date)]
        [CustomValidation(
            typeof(Patient),
            nameof(ValidateDateOfBirth))]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = ValidationMessages.GenderRequired)]
        public Gender Gender { get; set; }

        [Required(ErrorMessage = ValidationMessages.PhoneNumberRequired)]
        [StringLength(ValidationLimits.PhoneNumberLength)]
        [RegularExpression(RegexPatterns.PhoneNumber, ErrorMessage = ValidationMessages.InvalidPhoneNumberFormat)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = ValidationMessages.EmailRequired)]
        [EmailAddress(
            ErrorMessage = ValidationMessages.InvalidEmailFormat)]
        [StringLength(ValidationLimits.EmailLength)]
        public string Email { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public string? InsuranceId { get; set; }

        // Navigation Properties
        public virtual ICollection<Appointment> Appointments { get; set; }
            = new List<Appointment>();

        public virtual ICollection<HealthRecord> HealthRecords { get; set; }
            = new List<HealthRecord>();

        public int GetAge()
        {
            int age = DateTime.Today.Year - DateOfBirth.Year;

            if (DateOfBirth > DateTime.Today.AddYears(-age))
            {
                age--;
            }

            return age;
        }

        public static ValidationResult? ValidateDateOfBirth(
            DateTime date,
            ValidationContext context)
        {
            if (date.Year < 1900)
            {
                return new ValidationResult(
                    ValidationMessages.DateOfBirthYearMustBe1900OrLater);
            }

            if (date > DateTime.Today)
            {
                return new ValidationResult(
                    ValidationMessages.DateOfBirthCannotBeFuture);
            }

            return ValidationResult.Success;
        }
    }
}
