using HealthAxis.API.Models;
using HealthAxis.Shared.Common;
using HealthAxis.Shared.DTOs.AdminDtos;

namespace HealthAxis.API.Repositories.Interfaces
{
    public interface IAppointmentRepository : IRepository<Appointment>
    {
        Task<PagedResult<AppointmentReportDto>> GetAppointmentReportAsync(
            PaginationRequest request);
    }
}
