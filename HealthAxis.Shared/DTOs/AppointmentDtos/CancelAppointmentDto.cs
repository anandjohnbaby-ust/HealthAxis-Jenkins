using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace HealthAxis.Shared.DTOs.AppointmentDtos
{
    public class CancelAppointmentDto
    {
        
        public string? CancellationReason { get; set; } = string.Empty;
    }
}
