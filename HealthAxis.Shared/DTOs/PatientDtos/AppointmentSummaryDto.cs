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

        public TimeOnly TimeSlot { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}
