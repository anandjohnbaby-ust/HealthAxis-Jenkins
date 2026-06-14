using HealthAxis.API.Enums;
using HealthAxis.API.Utilities;
using Microsoft.AspNetCore.Components.Forms;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace HealthAxis.API.Models
{
    public class Appointment
    {

        [Key]
        public int AppointmentId { get; set; }

        [Required(ErrorMessage = ValidationMessages.PatientRequired)]
        public int PatientId { get; set; }

        [Required(ErrorMessage = ValidationMessages.DoctorRequired)]
        public int DoctorId { get; set; }

        [Required(ErrorMessage = ValidationMessages.AppointmentDateRequired)]
        [DataType(DataType.Date)]
        [CustomValidation(
            typeof(Appointment),
            nameof(ValidateScheduledDate))]
        public DateTime ScheduledDate { get; set; }

        [Required(ErrorMessage = ValidationMessages.TimeSlotRequired)]
        [StringLength(
            ValidationLimits.TimeSlotLength,
            ErrorMessage = ValidationMessages.InvalidTimeSlot)]
        public string TimeSlot { get; set; } = string.Empty;

        [Required(
            ErrorMessage = ValidationMessages.AppointmentStatusRequired)]
        public AppointmentStatus Status { get; set; }
            = AppointmentStatus.Pending;

        [StringLength(
            ValidationLimits.CancellationReasonLength)]
        public string? CancellationReason { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(PatientId))]
        public virtual Patient Patient { get; set; } = null!;

        [ForeignKey(nameof(DoctorId))]
        public virtual Doctor Doctor { get; set; } = null!;

        public virtual HealthRecord? HealthRecord { get; set; }


        // Methods
        public void Confirm()
        {

            Status = AppointmentStatus.Confirmed;
        }

        public void Cancel(string reason)
        {

            Status = AppointmentStatus.Cancelled;

            CancellationReason = reason;
        }

        public void Complete()
        {

            Status = AppointmentStatus.Completed;
        }

        public bool IsUpcoming()
        {

            return ScheduledDate.Date >= DateTime.Today
                   &&
                   Status != AppointmentStatus.Cancelled;
        }

        public bool IsCancelled()
        {

            return Status ==
                   AppointmentStatus.Cancelled;
        }

        public bool IsCompleted()
        {

            return Status ==
                   AppointmentStatus.Completed;
        }


        // Custom Validations
        public static ValidationResult? ValidateScheduledDate(
            DateTime scheduledDate,
            ValidationContext validationContext)
        {

            if (scheduledDate.Date < DateTime.Today)
            {

                return new ValidationResult(
                    ValidationMessages.ScheduledDateCannotBePast);
            }

            return ValidationResult.Success;
        }

    }
}
