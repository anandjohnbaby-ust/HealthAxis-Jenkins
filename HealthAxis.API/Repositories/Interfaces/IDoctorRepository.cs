using HealthAxis.API.Models;

namespace HealthAxis.API.Repositories.Interfaces
{
    public interface IDoctorRepository : IRepository<Doctor>
    {
        Task<Doctor?> GetAvailableDoctorByIdAsync(
                   int doctorId,
                   CancellationToken ct = default);

    }
}
