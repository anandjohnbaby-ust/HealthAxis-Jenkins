using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace HealthAxis.Shared.Utilities
{
    public static class PatientValidation
    {
        public static ValidationResult? ValidateDateOfBirth(
            DateTime? date,
            ValidationContext context)
        {
            if (!date.HasValue)
                return ValidationResult.Success;

            if (date.Value.Year < 1900)
            {
                return new ValidationResult(
                    ValidationMessages.DateOfBirthYearMustBe1900OrLater);
            }

            if (date.Value.Date > DateTime.Today)
            {
                return new ValidationResult(
                    ValidationMessages.DateOfBirthCannotBeFuture);
            }

            return ValidationResult.Success;
        }
    }
}
