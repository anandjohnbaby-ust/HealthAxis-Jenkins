using HealthAxis.API.DTOs.HealthRecordDtos;
using HealthAxis.API.Models;

namespace HealthAxis.API.Repositories.Interfaces
{
    public interface IHealthRecordRepository : IRepository<HealthRecord>
    {

        Task<IEnumerable<HealthRecord>> GetByPatientIdAsync(
                    int patientId,
                    CancellationToken ct = default);
    }
}

