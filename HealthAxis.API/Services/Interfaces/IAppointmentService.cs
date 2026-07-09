using HealthAxis.Shared.Common;
using HealthAxis.Shared.DTOs.AdminDtos;
using HealthAxis.Shared.DTOs.AppointmentDtos;

namespace HealthAxis.API.Services.Interfaces
{
    public interface IAppointmentService
    {
        Task<IEnumerable<AppointmentDto>> GetAllAsync(
            CancellationToken ct = default);

        Task<PagedResult<AppointmentReportDto>> GetAppointmentReportAsync(
            PaginationRequest request);

        Task<AppointmentDto> BookAppointmentAsync(
            CreateAppointmentDto dto,
            CancellationToken ct = default);

        Task<AppointmentDto> UpdateStatusAsync(
            int id,
            UpdateAppointmentStatusDto dto,
            CancellationToken ct = default);

        Task<AppointmentDto> DeleteAsync(
            int id,
            CancellationToken ct = default);

        Task<AppointmentDto> CancelAppointmentByPatientAsync(
                    int patientId,
                    int appointmentId,
                    CancelAppointmentDto dto,
                    CancellationToken ct = default);

        Task<List<TimeSlotDto>> GetAvailableSlotsAsync(
            int doctorId,
            DateTime date,
            CancellationToken ct = default);


    }
}