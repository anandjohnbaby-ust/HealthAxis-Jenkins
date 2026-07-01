using System;
using System.Collections.Generic;
using System.Text;

namespace HealthAxis.Shared.DTOs.PatientDtos
{
    public class AppointmentSummaryDto
    {
        public int AppointmentId { get; set; }

        public string DoctorName { get; set; } = string.Empty;

        public DateOnly ScheduledDate { get; set; }

        public string TimeSlot { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;
    }
}
