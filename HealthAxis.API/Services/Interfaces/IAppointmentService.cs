using HealthAxis.API.DTOs.AppointmentDtos;

namespace HealthAxis.API.Services.Interfaces
{
    public interface IAppointmentService
    {
        Task<IEnumerable<AppointmentDto>> GetAllAsync(
            CancellationToken ct = default);

        Task<AppointmentDto> AddAsync(
            CreateAppointmentDto dto,
            CancellationToken ct = default);

        Task<AppointmentDto> UpdateStatusAsync(
            int id,
            UpdateAppointmentStatusDto dto,
            CancellationToken ct = default);

        Task<AppointmentDto> DeleteAsync(
            int id,
            CancellationToken ct = default);
    }
}