using HealthAxis.API.Models;
using HealthAxis.Shared.Common;
using HealthAxis.Shared.DTOs.AdminDtos;
using HealthAxis.Shared.DTOs.AppointmentDtos;
using HealthAxis.Shared.DTOs.DoctorDtos;

namespace HealthAxis.API.Services.Interfaces
{
    public interface IAdminService
    {
        Task<IEnumerable<DoctorDto>> GetDoctors();

        Task<DoctorDto> CreateDoctor(CreateDoctorDto dto);

        Task<DoctorDto> UpdateDoctor(int doctorId, UpdateDoctorDto dto);

        Task<IEnumerable<AppointmentReportDto>> GetAppointmentReport();

        Task<PagedResult<UserManagementDto>> GetUsers(
            string? role,
            PaginationRequest request);

        Task<DashboardDto> GetDashboardAsync();

    }

}
