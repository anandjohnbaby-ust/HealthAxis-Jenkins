using HealthAxis.API.Models;
using HealthAxis.Shared.Common;
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

        Task<PagedResult<Doctor>> GetDoctorsAsync(
            PaginationRequest request,
            Specialisation? specialisation,
            string? search,
            CancellationToken ct = default);
    }
}
