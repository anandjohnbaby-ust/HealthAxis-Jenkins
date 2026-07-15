using HealthAxis.Shared.Common;
using HealthAxis.Shared.DTOs.AdminDtos;


namespace HealthAxis.API.Services.Interfaces
{
    public interface IAdminService
    {
        Task<DashboardDto> GetDashboardAsync();

        Task<PagedResult<AppointmentReportDto>> GetAppointmentReportAsync(
            PaginationRequest request);

    }
}