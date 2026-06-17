using HealthAxis.API.Data;
using HealthAxis.API.Models;
using HealthAxis.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HealthAxis.API.Repositories.Implementations
{
    public class PatientRepository : Repository<Patient>, IPatientRepository
    {
        public PatientRepository(ApplicationDbContext context) : base(context) {}


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
