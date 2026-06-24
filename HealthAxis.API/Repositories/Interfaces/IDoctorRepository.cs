using HealthAxis.API.Models;
using HealthAxis.Shared.Enums;

namespace HealthAxis.API.Repositories.Interfaces
{
    public interface IDoctorRepository : IRepository<Doctor>
    {
        Task<Doctor?> GetAvailableDoctorByIdAsync(
                   int doctorId,
                   CancellationToken ct = default);

        Task<IEnumerable<Doctor>> FilterBySpecialisationAsync(
                    Specialisation? specialisation,
                    CancellationToken ct = default);

        Task<IEnumerable<Doctor>> SearchAsync(
                    string searchTerm,
                    CancellationToken ct = default);

        Task<IEnumerable<Doctor>> GetDoctorsAsync(
                    Specialisation? specialisation,
                    string? search,
                    CancellationToken ct = default);
    }
}
