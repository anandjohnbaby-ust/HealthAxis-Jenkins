using HealthAxis.Shared.DTOs.HealthRecordDtos;
using HealthAxis.Shared.DTOs.PatientDtos;

namespace HealthAxis.API.Services.Interfaces
{
    public interface IPatientService
    {
        Task<IEnumerable<PatientDto>> GetPatientsAsync(
            string? search,
            CancellationToken ct = default);

        Task<PatientDto?> GetByIdAsync(
            int id,
            CancellationToken ct = default);

        Task<PatientDto> UpdateAsync(
            int id,
            UpdatePatientDto dto,
            CancellationToken ct = default);

        Task<IEnumerable<HealthRecordDto>> GetHealthRecordsByPatientId(
            int patientId,
            CancellationToken ct = default);
    }
}