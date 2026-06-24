using HealthAxis.Shared.Common;
using HealthAxis.Shared.DTOs.AdminDtos;

namespace HealthAxis.API.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<PagedResult<UserManagementDto>> GetUsersAsync(
            string? role,
            PaginationRequest request);
    }
}