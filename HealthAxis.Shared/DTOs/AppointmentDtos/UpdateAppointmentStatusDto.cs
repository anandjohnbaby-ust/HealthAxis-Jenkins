using HealthAxis.Shared.Enums;
using HealthAxis.Shared.Utilities;
using System.ComponentModel.DataAnnotations;

namespace HealthAxis.Shared.DTOs.AppointmentDtos
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