using HealthAxis.API.Repositories.Interfaces;
using HealthAxis.API.Services.Interfaces;
using HealthAxis.Shared.Common;
using HealthAxis.Shared.DTOs.AdminDtos;


namespace HealthAxis.API.Services.Implementations
{
    public class AdminService : IAdminService
    {
        private readonly IAdminRepository _adminRepository;
        private readonly IAppointmentRepository _appointmentRepository;


        public AdminService(
            IAdminRepository adminRepository,
            IAppointmentRepository appointmentRepository
            )
        {
            _adminRepository = adminRepository;
            _appointmentRepository = appointmentRepository;
        }

        #region DashBoard

        public async Task<DashboardDto> GetDashboardAsync()
        {
            return await _adminRepository.GetDashboardAsync();
        }

        #endregion

        // Get Appointment Report
        public async Task<PagedResult<AppointmentReportDto>> GetAppointmentReportAsync(
            PaginationRequest request)
        {
            return await _appointmentRepository.GetAppointmentReportAsync(request);
        }
    }
}