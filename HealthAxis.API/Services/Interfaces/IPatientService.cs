using HealthAxis.Shared.Common;
using HealthAxis.Shared.DTOs.AppointmentDtos;
using HealthAxis.Shared.DTOs.DoctorDtos;
using HealthAxis.Shared.DTOs.HealthRecordDtos;
using HealthAxis.Shared.DTOs.PatientDtos;
using HealthAxis.Shared.Enums;

namespace HealthAxis.API.Services.Interfaces
{
    public interface IPatientService
    {
        Task<PagedResult<PatientDto>> GetPatientsAsync(
            PaginationRequest request,
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

        Task<IEnumerable<AppointmentDto>> GetAppointmentsByPatientIdAsync(
            int patientId,
            CancellationToken ct = default);

        Task<PatientDashboardDto> GetDashboardAsync(
            int patientId,
            CancellationToken ct = default);
    }
}