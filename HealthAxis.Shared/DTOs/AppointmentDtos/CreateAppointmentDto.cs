using HealthAxis.Shared.Utilities;
using System.ComponentModel.DataAnnotations;

namespace HealthAxis.Shared.DTOs.AppointmentDtos
{
    public class CreateAppointmentDto
    {
        [Required(ErrorMessage = ValidationMessages.PatientRequired)]
        public int PatientId { get; set; }

        [Required(ErrorMessage = ValidationMessages.DoctorRequired)]
        public int DoctorId { get; set; }

        [Required(ErrorMessage = ValidationMessages.AppointmentDateRequired)]
        [DataType(DataType.Date)]
        public DateTime ScheduledDate { get; set; }

        //[Required(ErrorMessage = ValidationMessages.TimeSlotRequired)]
        //[StringLength(
        //    ValidationLimits.TimeSlotLength,
        //    ErrorMessage = ValidationMessages.InvalidTimeSlot)]
        public TimeOnly TimeSlot { get; set; }
    }
}