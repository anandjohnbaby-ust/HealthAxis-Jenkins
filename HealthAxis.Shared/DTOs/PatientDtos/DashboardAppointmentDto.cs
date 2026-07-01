using HealthAxis.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace HealthAxis.Shared.DTOs.PatientDtos
{
    public class DashboardAppointmentDto
    {
        public int AppointmentId { get; set; }

        public string DoctorName { get; set; } = string.Empty;

        public DateTime ScheduledDate { get; set; }

        public string TimeSlot { get; set; } = string.Empty;

        public AppointmentStatus Status { get; set; }
    }
}
