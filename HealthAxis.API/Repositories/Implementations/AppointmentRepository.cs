using HealthAxis.API.Data;
using HealthAxis.API.Models;
using HealthAxis.API.Repositories.Interfaces;
using HealthAxis.Shared.DTOs.AdminDtos;
using HealthAxis.Shared.Enums;
using Microsoft.EntityFrameworkCore;
namespace HealthAxis.API.Repositories.Implementations
{
    public class AppointmentRepository : Repository<Appointment>, IAppointmentRepository
    {
        public AppointmentRepository(ApplicationDbContext context) : base(context)
        { }
        public async Task<IEnumerable<AppointmentReportDto>> GetAppointmentReportAsync()
            {
                return await _context.Appointments
                    .GroupBy(a => a.ScheduledDate.Date)
                    .Select(group => new AppointmentReportDto
                    {
                        Date = DateOnly.FromDateTime(group.Key),

                        ConfirmedCount = group.Count(a =>
                            a.Status == AppointmentStatus.Confirmed),
                        PendingCount = group.Count(a =>
                            a.Status == AppointmentStatus.Pending),

                        CancelledCount = group.Count(a =>
                            a.Status == AppointmentStatus.Cancelled),

                        CompletedCount = group.Count(a =>
                            a.Status == AppointmentStatus.Completed),

                        TotalAppointments = group.Count()
                    })
                    .OrderBy(r => r.Date)
                    .ToListAsync();
            }
    }
}