using HealthAxis.API.Data;
using HealthAxis.API.Models;
using HealthAxis.API.Repositories.Interfaces;
using HealthAxis.Shared.Enums;
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

        public async Task<IEnumerable<Doctor>> FilterBySpecialisationAsync(
                Specialisation? specialisation,
                CancellationToken ct = default)
        {
            var query = _context.Doctors.AsQueryable();

            if (specialisation.HasValue)
            {
                query = query.Where(d =>
                    d.Specialisation == specialisation.Value);
            }

            return await query
                .OrderBy(d => d.FullName)
                .ToListAsync(ct);
        }

        public async Task<IEnumerable<Doctor>> SearchAsync(
            string searchTerm,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await _context.Doctors
                    .OrderBy(d => d.FullName)
                    .ToListAsync(ct);
            }

            searchTerm = searchTerm.Trim();

            int.TryParse(searchTerm, out int doctorId);

            return await _context.Doctors
                .Where(d =>
                    (doctorId > 0 && d.DoctorId == doctorId) ||
                    EF.Functions.Like(d.FullName, $"%{searchTerm}%"))
                .OrderBy(d => d.FullName)
                .ToListAsync(ct);
        }

        public async Task<IEnumerable<Doctor>> GetDoctorsAsync(
    Specialisation? specialisation,
    string? search,
    CancellationToken ct = default)
        {
            IQueryable<Doctor> query = _context.Doctors;

            if (specialisation.HasValue)
            {
                query = query.Where(d =>
                    d.Specialisation == specialisation.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                int.TryParse(search, out int doctorId);

                query = query.Where(d =>
                    (doctorId > 0 && d.DoctorId == doctorId) ||
                    EF.Functions.Like(d.FullName, $"%{search}%"));
            }

            return await query
                .OrderBy(d => d.FullName)
                .ToListAsync(ct);
        }
    }
}
