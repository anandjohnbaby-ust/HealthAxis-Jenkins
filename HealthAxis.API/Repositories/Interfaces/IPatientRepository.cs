using HealthAxis.API.Models;

namespace HealthAxis.API.Repositories.Interfaces
{
    public interface IPatientRepository : IRepository<Patient>
    {

        Task<Patient?> GetHealthRecordsByPatientId(int patientId,
        CancellationToken cancellationToken = default);

        Task<IEnumerable<Patient>> GetPatientsAsync(
            string? search,
            CancellationToken ct = default);
    }
}