using HealthAxis.API.Data;
using HealthAxis.API.Models;
using HealthAxis.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
namespace HealthAxis.API.Repositories.Implementations
{
    public class DoctorRepository : Repository<Doctor>, IDoctorRepository
    {
        public DoctorRepository(ApplicationDbContext context) : base(context) {}

        public async Task<IEnumerable<Doctor>> GetAvailableDoctorsAsync()
        {
            return await _context.Doctors
                .Where(d => d.IsActive)
                .ToListAsync();
        }
    }
}
