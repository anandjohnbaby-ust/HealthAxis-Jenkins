using HealthAxis.API.Repositories.Interfaces;
using HealthAxis.API.Services.Interfaces;
using HealthAxis.Shared.Common;
using HealthAxis.Shared.DTOs.AdminDtos;
using HealthAxis.Shared.DTOs.DoctorDtos;
using HealthAxis.Shared.DTOs.PatientDtos;
using HealthAxis.Shared.Enums;

namespace HealthAxis.API.Services.Implementations
{
    public class AdminService : IAdminService
    {
        private readonly IAdminRepository _adminRepository;


        public AdminService(
            IAdminRepository adminRepository
            )
        {
            _adminRepository = adminRepository;
        }

        #region DashBoard

        public async Task<DashboardDto> GetDashboardAsync()
        {
            return await _adminRepository.GetDashboardAsync();
        }

        #endregion
    }
}