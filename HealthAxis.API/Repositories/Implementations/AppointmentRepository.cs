using HealthAxis.API.Data;
using HealthAxis.API.Models;
using HealthAxis.API.Repositories.Interfaces;
using HealthAxis.Shared.Common;
using HealthAxis.Shared.DTOs.AdminDtos;
using HealthAxis.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace HealthAxis.API.Repositories.Implementations
{
    public class AppointmentRepository : Repository<Appointment>, IAppointmentRepository
    {
        public AppointmentRepository(
            ApplicationDbContext context)
            : base(context)
        {
        }

        public async Task<PagedResult<AppointmentReportDto>> GetAppointmentReportAsync(
            PaginationRequest request)
        {
            var query = _context.Appointments
                .AsNoTracking()
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
                });

            int totalCount = await query.CountAsync();

            var reports = await query
                .OrderBy(r => r.Date)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            return new PagedResult<AppointmentReportDto>
            {
                Items = reports,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        public async Task<bool> IsTimeSlotBookedAsync(
            int doctorId,
            DateTime scheduledDate,
            TimeOnly timeSlot,
            CancellationToken ct = default)
        {
            return await _context.Appointments.AnyAsync(a =>
                a.DoctorId == doctorId &&
                a.ScheduledDate.Date == scheduledDate.Date &&
                a.TimeSlot == timeSlot &&
                a.Status != AppointmentStatus.Cancelled,
                ct);
        }

        public async Task<List<TimeOnly>> GetBookedTimeSlotsAsync(
            int doctorId,
            DateTime date,
            CancellationToken ct = default)
        {
            return await _context.Appointments
                .Where(a =>
                    a.DoctorId == doctorId &&
                    a.ScheduledDate.Date == date.Date &&
                    a.Status != AppointmentStatus.Cancelled)
                .Select(a => a.TimeSlot)
                .ToListAsync(ct);
        }

        public async Task<int> GetPatientDoctorAppointmentCountAsync(
            int patientId,
            int doctorId,
            DateTime scheduledDate,
            CancellationToken ct = default)
        {
            return await _context.Appointments
                .AsNoTracking()
                .CountAsync(a =>
                    a.PatientId == patientId &&
                    a.DoctorId == doctorId &&
                    a.ScheduledDate.Date == scheduledDate.Date &&
                    a.Status != AppointmentStatus.Cancelled,
                    ct);
        }
    }
}