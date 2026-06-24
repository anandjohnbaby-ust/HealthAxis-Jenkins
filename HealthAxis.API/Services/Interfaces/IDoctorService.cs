using HealthAxis.Shared.Common;
using HealthAxis.Shared.DTOs.DoctorDtos;
using HealthAxis.Shared.Enums;

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

        Task<IEnumerable<DoctorDto>> FilterBySpecialisationAsync(
            Specialisation? specialisation,
            CancellationToken ct = default);

        Task<IEnumerable<DoctorDto>> SearchAsync(
            string searchTerm,
            CancellationToken ct = default);

        Task<PagedResult<DoctorDto>> GetDoctorsAsync(
            PaginationRequest request,
            Specialisation? specialisation,
            string? search,
            CancellationToken ct = default);
    }
}