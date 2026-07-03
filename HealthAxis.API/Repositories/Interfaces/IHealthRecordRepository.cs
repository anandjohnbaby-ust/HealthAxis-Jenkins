using HealthAxis.API.Models;

namespace HealthAxis.API.Repositories.Interfaces
{
    public interface IHealthRecordRepository : IRepository<HealthRecord>
    {

        Task<IEnumerable<HealthRecord>> GetByPatientIdAsync(
            int patientId,
            CancellationToken ct = default);

        Task<IEnumerable<HealthRecord>> GetPatientHealthRecordsAsync(
            int patientId,
            CancellationToken ct = default);
    }
}

