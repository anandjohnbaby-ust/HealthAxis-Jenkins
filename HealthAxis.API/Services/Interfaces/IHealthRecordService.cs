
namespace HealthAxis.API.Services.Interfaces
{
    public interface IHealthRecordService
    {
        Task<IEnumerable<HealthRecordDto>>
            GetByPatientIdAsync(
                int patientId,
                CancellationToken ct = default);

        Task<HealthRecordDto>
            GetByIdAsync(
                int id,
                CancellationToken ct = default);

        Task<HealthRecordDto>
            AddAsync(
                CreateHealthRecordDto dto,
                CancellationToken ct = default);
    }
}