using HealthAxis.API.Data;
using HealthAxis.API.Models;
using HealthAxis.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HealthAxis.API.Repositories.Implementations
{
    public class HealthRecordRepository : Repository<HealthRecord>, IHealthRecordRepository
    {
        public HealthRecordRepository(ApplicationDbContext context) : base(context) {}

        public async Task<IEnumerable<HealthRecord>> GetByPatientIdAsync(
            int patientId,
            CancellationToken ct = default)
        {
            return await _context.HealthRecords
                .Where(hr => hr.PatientId == patientId)
                .OrderByDescending(hr => hr.VisitDate)
                .ToListAsync(ct);
        }
    }
 }

