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

        public async Task<PagedResult<Doctor>> GetAvailableDoctorsAsync(
            Specialisation? specialisation,
            string? search,
            PaginationRequest request,
            CancellationToken ct = default)
        {
            var query = _context.Doctors
                .AsNoTracking()
                .Include(d => d.User)
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

            var totalCount = await query.CountAsync(ct);

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

        // Search, filter, pagination all doctors
        public async Task<PagedResult<Doctor>> GetDoctorsAsync(
            PaginationRequest request,
            Specialisation? specialisation,
            string? search,
            bool? isActive,
            CancellationToken ct = default)
        {
            IQueryable<Doctor> query = _context.Doctors.AsNoTracking();

            // Filter by Specialisation
            if (specialisation.HasValue)
            {
                query = query.Where(d =>
                    d.Specialisation == specialisation.Value);
            }

            // Filter by Active / Inactive
            if (isActive.HasValue)
            {
                query = query.Where(d =>
                    d.IsActive == isActive.Value);
            }

            // Search by Doctor ID or Name
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
                .Include(d => d.User)
                .FirstOrDefaultAsync(
                    d => d.UserId == userId,
                    cancellationToken);
        }

        // ===================================================
        // Doctor Dashboard
        // ===================================================

        public async Task<PagedResult<Appointment>> GetAppointmentsAsync(
             int doctorId,
             PaginationRequest request,
             string? search = null,
             AppointmentStatus? status = null,
             DateTime? date = null,
             CancellationToken ct = default)
        {
            var query = _context.Appointments
                .AsNoTracking()
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Include(a => a.HealthRecord)
                .Where(a => a.DoctorId == doctorId);

            // Search by Patient Name or Patient ID
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(a =>
                    a.Patient.FullName.Contains(search) ||
                    a.Patient.PatientId.ToString().Contains(search));
            }

            // Filter by Status
            if (status.HasValue)
            {
                query = query.Where(a => a.Status == status.Value);
            }

            // Filter by Date
            if (date.HasValue)
            {
                query = query.Where(a =>
                    a.ScheduledDate.Date == date.Value.Date);
            }

            var totalCount = await query.CountAsync(ct);

            var appointments = await query
                .OrderBy(a => a.ScheduledDate)
                .ThenBy(a => a.TimeSlot)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(ct);

            return new PagedResult<Appointment>
            {
                Items = appointments,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        public async Task<PagedResult<Appointment>> GetTodaysAppointmentsAsync(
            int doctorId,
            DateTime today,
            PaginationRequest request,
            string? search = null,
            AppointmentStatus? status = null,
            CancellationToken ct = default)
        {
            var query = _context.Appointments
                .AsNoTracking()
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Include(a => a.HealthRecord)
                .Where(a =>
                    a.DoctorId == doctorId &&
                    a.ScheduledDate.Date == today.Date);

            // ==========================
            // Search by Patient Name / ID
            // ==========================

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(a =>
                    a.Patient.FullName.Contains(search) ||
                    a.Patient.PatientId.ToString().Contains(search));
            }

            // ==========================
            // Filter by Status
            // ==========================

            if (status.HasValue)
            {
                query = query.Where(a => a.Status == status.Value);
            }

            var totalCount = await query.CountAsync(ct);

            var appointments = await query
                .OrderBy(a => a.TimeSlot)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(ct);

            return new PagedResult<Appointment>
            {
                Items = appointments,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        public async Task<PagedResult<Appointment>> GetWeeklyAppointmentsAsync(
            int doctorId,
            PaginationRequest request,
            string? search = null,
            AppointmentStatus? status = null,
            DateTime? date = null,
            CancellationToken ct = default)
        {
            DateTime today = DateTime.Today;

            int diff = today.DayOfWeek == DayOfWeek.Sunday
                ? 6
                : (int)today.DayOfWeek - 1;

            DateTime startOfWeek = today.AddDays(-diff);
            DateTime endOfWeek = startOfWeek.AddDays(6);

            var query = _context.Appointments
                .AsNoTracking()
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Include(a => a.HealthRecord)
                .Where(a =>
                    a.DoctorId == doctorId &&
                    a.ScheduledDate.Date >= startOfWeek.Date &&
                    a.ScheduledDate.Date <= endOfWeek.Date);

            // ==========================
            // Search by Patient Name / ID
            // ==========================

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(a =>
                    a.Patient.FullName.Contains(search) ||
                    a.Patient.PatientId.ToString().Contains(search));
            }

            // ==========================
            // Filter by Status
            // ==========================

            if (status.HasValue)
            {
                query = query.Where(a => a.Status == status.Value);
            }

            // ==========================
            // Filter by Date
            // ==========================

            if (date.HasValue)
            {
                query = query.Where(a =>
                    a.ScheduledDate.Date == date.Value.Date);
            }

            var totalCount = await query.CountAsync(ct);

            var appointments = await query
                .OrderBy(a => a.ScheduledDate)
                .ThenBy(a => a.TimeSlot)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(ct);

            return new PagedResult<Appointment>
            {
                Items = appointments,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
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

                    UpcomingAppointments = d.Appointments
                        .Where(a =>
                            a.ScheduledDate >= today &&
                            (a.Status == AppointmentStatus.Pending ||
                             a.Status == AppointmentStatus.Confirmed))
                        .OrderBy(a => a.ScheduledDate)
                        .ThenBy(a => a.TimeSlot)
                        .Take(2)
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
