using HealthAxis.API.Enums;
using HealthAxis.API.Utilities;
using System.ComponentModel.DataAnnotations;

namespace HealthAxis.API.DTOs.AppointmentDtos
{
    public class UpdateAppointmentStatusDto
    {
        [Required(
            ErrorMessage = ValidationMessages.AppointmentStatusRequired)]
        public AppointmentStatus Status { get; set; }

        [StringLength(
            ValidationLimits.CancellationReasonLength)]
        public string? CancellationReason { get; set; }
    }
}