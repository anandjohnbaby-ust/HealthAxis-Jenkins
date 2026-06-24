using HealthAxis.API.Models;
using HealthAxis.Shared.Common;

namespace HealthAxis.API.Repositories.Interfaces
{
    public interface IPatientRepository : IRepository<Patient>
    {

        Task<Patient?> GetHealthRecordsByPatientId(int patientId,
        CancellationToken cancellationToken = default);

        Task<PagedResult<Patient>> GetPatientsAsync(
            PaginationRequest request,
            string? search,
            CancellationToken ct = default);
    }
}