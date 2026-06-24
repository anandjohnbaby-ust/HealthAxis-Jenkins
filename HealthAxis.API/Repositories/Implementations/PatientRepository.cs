using HealthAxis.API.Data;
using HealthAxis.API.Models;
using HealthAxis.API.Repositories.Interfaces;
using HealthAxis.Shared.Common;
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

                int.TryParse(search, out int patientId);

                query = query.Where(p =>
                    (patientId > 0 && p.PatientId == patientId) ||
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

        // Get HealthRecords for a Patient by PatientId
        public async Task<Patient?> GetHealthRecordsByPatientId(
            int patientId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Patients
                .Include(p => p.HealthRecords)
                .FirstOrDefaultAsync(
                    p => p.PatientId == patientId,
                    cancellationToken);
        }
    }
}
