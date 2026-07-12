using HealthAxis.API.Data;
using HealthAxis.API.Models;
using HealthAxis.API.Repositories.Interfaces;
using HealthAxis.Shared.Common;
using HealthAxis.Shared.DTOs.PatientDtos;
using HealthAxis.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace HealthAxis.API.Repositories.Implementations
{
    public class PatientRepository : Repository<Patient>, IPatientRepository
    {
        public PatientRepository(ApplicationDbContext context) : base(context) {}

        public async Task<PagedResult<Patient>> GetPatientsAsync(
            PaginationRequest request,
            string? search,
            CancellationToken ct = default)
        {
            IQueryable<Patient> query = _context.Patients.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                bool isPatientId = int.TryParse(search, out int patientId);

                query = query.Where(p =>
                    (isPatientId && p.PatientId == patientId) ||
                    EF.Functions.Like(p.FullName, $"%{search}%") ||
                    EF.Functions.Like(p.Email, $"%{search}%"));
            }

            int totalCount = await query.CountAsync(ct);

            var patients = await query
                .OrderBy(p => p.FullName)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(ct);

            return new PagedResult<Patient>
            {
                Items = patients,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        public async Task<PagedResult<HealthRecord>> GetHealthRecordsByPatientIdAsync(
            int patientId,
            PaginationRequest request,
            CancellationToken cancellationToken = default)
        {
            var query = _context.HealthRecords
                .AsNoTracking()
                .Include(hr => hr.Doctor)
                .Where(hr => hr.PatientId == patientId)
                .OrderByDescending(hr => hr.VisitDate);

            int totalCount = await query.CountAsync(cancellationToken);

            var records = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<HealthRecord>
            {
                Items = records,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        public async Task<Patient?> GetByUserIdAsync(
            string userId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Patients
                .FirstOrDefaultAsync(
                    p => p.UserId == userId,
                    cancellationToken);
        }
        public async Task<PagedResult<Appointment>> GetAppointmentsByPatientIdAsync(
           int patientId,
           PaginationRequest request,
           CancellationToken ct = default)
        {
            var query = _context.Appointments
                .AsNoTracking()
                .Where(a => a.PatientId == patientId)
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .Include(a => a.HealthRecord)   // <-- Add this
                .OrderByDescending(a => a.ScheduledDate)
                .ThenBy(a => a.TimeSlot);

            int totalCount = await query.CountAsync(ct);

            var appointments = await query
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

        public async Task<PatientDashboardDto?> GetDashboardAsync(
            int patientId,
            CancellationToken ct = default)
        {
            return await _context.Patients
                .Where(p => p.PatientId == patientId)
                .Select(p => new PatientDashboardDto
                {
                    FullName = p.FullName,

                    TotalAppointments = p.Appointments.Count,

                    TotalHealthRecords = p.HealthRecords.Count,

                    NextAppointment = p.Appointments
                        .Where(a =>
                            a.Status != AppointmentStatus.Cancelled &&
                            a.ScheduledDate >= DateTime.Today)
                        .OrderBy(a => a.ScheduledDate)
                        .ThenBy(a => a.TimeSlot)
                        .Select(a => new DashboardAppointmentDto
                        {
                            AppointmentId = a.AppointmentId,
                            DoctorName = a.Doctor.FullName,
                            ScheduledDate = a.ScheduledDate,
                            TimeSlot = a.TimeSlot,
                            Status = a.Status
                        })
                        .FirstOrDefault()
                })
                .FirstOrDefaultAsync(ct);
        }

    }
}
