using HealthAxis.Shared.DTOs.AdminDtos;

namespace HealthAxis.API.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<IEnumerable<UserManagementDto>> GetUsersAsync(string? role);
    }
}