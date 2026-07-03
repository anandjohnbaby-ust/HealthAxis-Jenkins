using HealthAxis.Shared.Common;
using HealthAxis.Shared.DTOs.AdminDtos;
using Microsoft.JSInterop;
using System.Net.Http.Json;

namespace HealthAxis.Admin.Services
{
    public class AdminService : AuthorizedApiServiceBase
    {
        public AdminService(HttpClient http, IJSRuntime js) : base(http, js)
        {
        }

        public async Task<PagedResult<AppointmentReportDto>?> GetAppointmentReportAsync(
            int pageNumber = 1,
            int pageSize = 10)
        {
            await AttachAuthHeaderAsync();

            var url = $"api/admin/reports/appointments?pageNumber={pageNumber}&pageSize={pageSize}";

            return await Http.GetFromJsonAsync<PagedResult<AppointmentReportDto>>(url);
        }

        public async Task<DashboardDto> GetDashboardAsync()
        {
            await AttachAuthHeaderAsync();

            return await Http.GetFromJsonAsync<DashboardDto>("api/admin/dashboard")
                   ?? new DashboardDto();
        }
    }
}