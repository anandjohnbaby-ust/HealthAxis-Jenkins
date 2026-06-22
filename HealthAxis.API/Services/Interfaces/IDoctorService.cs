using HealthAxis.Shared.DTOs.DoctorDtos;

namespace HealthAxis.API.Services.Interfaces
{
    public interface IDoctorService
    {
        Task<IEnumerable<DoctorDto>> GetAllAsync(
            CancellationToken ct = default);

        Task<DoctorDto?> GetByIdAsync(
            int id,
            CancellationToken ct = default);

        Task<DoctorDto> GetAvailableDoctorByIdAsync(
            int doctorId,
            CancellationToken ct = default);
    }
}