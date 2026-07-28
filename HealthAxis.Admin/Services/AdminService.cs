using HealthAxis.Shared.Common;
using HealthAxis.Shared.DTOs.AdminDtos;
using System.Net.Http.Json;
using System.Text.Json;

namespace HealthAxis.Admin.Services
{
    public class AdminService
    {
        private static readonly JsonSerializerOptions DashboardJsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly HttpClient _http;

        public AdminService(HttpClient http)
        {
            _http = http;
        }

        public async Task<PagedResult<AppointmentReportDto>?> GetAppointmentReportAsync(
      int pageNumber = 1,
      int pageSize = 10)
        {
            var url = $"api/admin/reports/appointments?pageNumber={pageNumber}&pageSize={pageSize}";
            return await _http.GetFromJsonAsync<PagedResult<AppointmentReportDto>>(url);
        }

        public async Task<DashboardDto> GetDashboardAsync()
        {
            var response = await _http.GetAsync("api/admin/dashboard");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var dashboard = JsonSerializer.Deserialize<DashboardDto>(content, DashboardJsonOptions);

            if (dashboard == null)
                throw new InvalidOperationException("DashboardDto deserialized to null.");

            return dashboard;
        }
    }
}