using HealthAxis.API.Data;
using HealthAxis.API.Models;
using HealthAxis.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
namespace HealthAxis.API.Repositories.Implementations
{
    public class DoctorRepository : Repository<Doctor>, IDoctorRepository
    {
        public DoctorRepository(ApplicationDbContext context) : base(context) {}

        public async Task<Doctor?> GetAvailableDoctorByIdAsync(
            int doctorId,
            CancellationToken ct = default)
        {
            return await _context.Doctors
                .FirstOrDefaultAsync(
                    d => d.DoctorId == doctorId &&
                         d.IsActive,
                    ct);
        }
    }
}
