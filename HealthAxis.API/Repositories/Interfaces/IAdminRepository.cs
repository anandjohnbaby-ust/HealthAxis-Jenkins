using HealthAxis.Shared.DTOs.AdminDtos;

namespace HealthAxis.API.Repositories.Interfaces
{
    public interface IAdminRepository
    {
        Task<DashboardDto> GetDashboardAsync();
    }
}