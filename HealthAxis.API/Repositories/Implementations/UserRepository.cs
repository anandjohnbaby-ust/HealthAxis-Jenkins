using HealthAxis.API.Data;
using HealthAxis.API.Models;
using HealthAxis.API.Repositories.Interfaces;
using HealthAxis.Shared.DTOs.AdminDtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HealthAxis.API.Repositories.Implementations
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public UserRepository(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IEnumerable<UserManagementDto>> GetUsersAsync(string? role)
        {
            var users = await _userManager.Users
                .Include(u => u.Doctor)
                .Include(u => u.Patient)
                .ToListAsync();

            var result = new List<UserManagementDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                var userRole = roles.FirstOrDefault() ?? string.Empty;

                if (!string.IsNullOrWhiteSpace(role) &&
                    !userRole.Equals(role, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                result.Add(new UserManagementDto
                {
                    UserId = user.Id,
                    Email = user.Email ?? "",
                    PhoneNumber = user.PhoneNumber,
                    EmailConfirmed = user.EmailConfirmed,
                    Role = userRole,
                    FullName = user.Doctor?.FullName
                               ?? user.Patient?.FullName
                               ?? "Admin",
                    IsActive = user.Doctor?.IsActive ?? true
                });
            }

            return result;
        }
    }
}