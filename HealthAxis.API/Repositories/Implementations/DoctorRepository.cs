using HealthAxis.API.Data;
using HealthAxis.API.Models;
using HealthAxis.API.Repositories.Interfaces;
using HealthAxis.Shared.Common;
using HealthAxis.Shared.DTOs.DoctorDtos;
using HealthAxis.Shared.Enums;
using Microsoft.EntityFrameworkCore;
namespace HealthAxis.API.Repositories.Implementations
{
    public class DoctorRepository : Repository<Doctor>, IDoctorRepository
    {
        public DoctorRepository(ApplicationDbContext context) : base(context) { }


        public async Task<Doctor> CreateDoctorAsync(
            Doctor doctor,
            CancellationToken ct = default)
        {
            await _context.Doctors.AddAsync(doctor, ct);

            await _context.SaveChangesAsync(ct);

            return doctor;
        }

        public async Task<IEnumerable<Doctor>> GetAvailableDoctorsAsync(
            Specialisation? specialisation,
            string? search,
            CancellationToken ct = default)
        {
            var query = _context.Doctors
                .Where(d => d.IsActive)
                .AsQueryable();

            if (specialisation.HasValue)
            {
                query = query.Where(
                    d => d.Specialisation == specialisation.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(d =>
                    d.FullName.Contains(search));
            }

            return await query.ToListAsync(ct);
        }

        // Search, filter, pagenation all doctors
        public async Task<PagedResult<Doctor>> GetDoctorsAsync(
            PaginationRequest request,
            Specialisation? specialisation,
            string? search,
            CancellationToken ct = default)
        {
            IQueryable<Doctor> query = _context.Doctors.AsNoTracking();

            if (specialisation.HasValue)
            {
                query = query.Where(d =>
                    d.Specialisation == specialisation.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                bool isDoctorId = int.TryParse(search, out int doctorId);

                query = query.Where(d =>
                    (isDoctorId && d.DoctorId == doctorId) ||
                    EF.Functions.Like(d.FullName, $"%{search}%"));
            }

            int totalCount = await query.CountAsync(ct);

            var doctors = await query
                .OrderBy(d => d.FullName)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(ct);

            return new PagedResult<Doctor>
            {
                Items = doctors,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        public async Task<Doctor?> GetByUserIdAsync(
            string userId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Doctors
                .FirstOrDefaultAsync(
                    d => d.UserId == userId,
                    cancellationToken);
        }

        // ===================================================
        // Doctor Dashboard
        // ===================================================

        public async Task<IEnumerable<Appointment>> GetAppointmentsAsync(
            int doctorId,
            CancellationToken ct = default)
        {
            return await _context.Appointments
                .AsNoTracking()
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Include(a => a.HealthRecord)
                .Where(a => a.DoctorId == doctorId)
                .OrderBy(a => a.ScheduledDate)
                .ThenBy(a => a.TimeSlot)
                .ToListAsync(ct);
        }

        public async Task<IEnumerable<Appointment>> GetTodaysAppointmentsAsync(
            int doctorId,
            DateTime today,
            CancellationToken ct = default)
        {
            return await _context.Appointments
                .AsNoTracking()
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Include(a => a.HealthRecord)
                .Where(a =>
                    a.DoctorId == doctorId &&
                    a.ScheduledDate.Date == today.Date)
                .OrderBy(a => a.TimeSlot)
                .ToListAsync(ct);
        }

        public async Task<IEnumerable<Appointment>> GetWeeklyAppointmentsAsync(
            int doctorId,
            DateTime startDate,
            DateTime endDate,
            CancellationToken ct = default)
        {
            return await _context.Appointments
                .AsNoTracking()
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Include(a => a.HealthRecord)
                .Where(a =>
                    a.DoctorId == doctorId &&
                    a.ScheduledDate.Date >= startDate.Date &&
                    a.ScheduledDate.Date <= endDate.Date)
                .OrderBy(a => a.ScheduledDate)
                .ThenBy(a => a.TimeSlot)
                .ToListAsync(ct);
        }

        public async Task<DoctorDashboardDto?> GetDashboardAsync(
            int doctorId,
            CancellationToken ct = default)
        {
            var today = DateTime.Today;
            var weekEnd = today.AddDays(7);

            return await _context.Doctors
                .Where(d => d.DoctorId == doctorId)
                .Select(d => new DoctorDashboardDto
                {
                    FullName = d.FullName,

                    TodayAppointments = d.Appointments.Count(a =>
                        a.ScheduledDate.Date == today),

                    WeeklyAppointments = d.Appointments.Count(a =>
                        a.ScheduledDate >= today &&
                        a.ScheduledDate < weekEnd),

                    TotalAppointments = d.Appointments.Count,

                    TodaySchedule = d.Appointments
                        .Where(a => a.ScheduledDate.Date == today)
                        .OrderBy(a => a.TimeSlot)
                        .Select(a => new TodayAppointmentDto
                        {
                            AppointmentId = a.AppointmentId,
                            PatientName = a.Patient.FullName,
                            ScheduledDate = a.ScheduledDate,
                            TimeSlot = a.TimeSlot,
                            Status = a.Status
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync(ct);
        }

    }
}
