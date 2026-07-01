using System;
using System.Collections.Generic;
using System.Text;

namespace HealthAxis.Shared.DTOs.DoctorDtos
{
    public class DoctorDashboardDto
    {
        public string FullName { get; set; } = string.Empty;

        public int TodayAppointments { get; set; }

        public int WeeklyAppointments { get; set; }

        public int TotalAppointments { get; set; }

        public List<TodayAppointmentDto> TodaySchedule { get; set; } = [];
    }
}
