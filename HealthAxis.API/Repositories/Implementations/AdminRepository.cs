using HealthAxis.API.Data;
using HealthAxis.API.Repositories.Interfaces;
using HealthAxis.Shared.DTOs.AdminDtos;
using Microsoft.EntityFrameworkCore;

namespace HealthAxis.API.Repositories.Implementations
{
    public class AdminRepository : IAdminRepository
    {
        private readonly ApplicationDbContext _context;

        public AdminRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardDto> GetDashboardAsync()
        {
            return new DashboardDto
            {
                TotalUsers = await _context.Users.CountAsync(),
                TotalDoctors = await _context.Doctors.CountAsync(),
                TotalPatients = await _context.Patients.CountAsync(),
                TotalAppointments = await _context.Appointments.CountAsync()
            };
        }
    }
}