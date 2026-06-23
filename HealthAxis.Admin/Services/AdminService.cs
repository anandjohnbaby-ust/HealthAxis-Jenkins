using System.Net.Http.Json;
using System.Net.Http.Headers;
using HealthAxis.Shared.DTOs.AdminDtos;
using Microsoft.JSInterop;

namespace HealthAxis.Admin.Services
{
    public class AdminService
    {
        private readonly HttpClient _http;
        private readonly IJSRuntime _js;

        public AdminService(HttpClient http, IJSRuntime js)
        {
            _http = http;
            _js = js;
        }

        private async Task AttachAuthHeaderAsync()
        {
            try
            {
                var token = await _js.InvokeAsync<string>("localStorage.getItem", "token");
                if (!string.IsNullOrWhiteSpace(token))
                {
                    token = token.Trim('"');
                    _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
                else
                {
                    _http.DefaultRequestHeaders.Authorization = null;
                }
            }
            catch
            {
                // ignore and proceed without auth header
                _http.DefaultRequestHeaders.Authorization = null;
            }
        }

        public async Task<List<UserManagementDto>> GetUsersAsync(string? role = null)
        {
            var url = "api/admin/users";

            if (!string.IsNullOrWhiteSpace(role))
            {
                url += $"?role={role}";
            }

            await AttachAuthHeaderAsync();
            return await _http.GetFromJsonAsync<List<UserManagementDto>>(url)
                   ?? new List<UserManagementDto>();
        }

        public async Task<List<AppointmentReportDto>> GetAppointmentReportAsync()
        {
            await AttachAuthHeaderAsync();
            return await _http.GetFromJsonAsync<List<AppointmentReportDto>>
                ("api/admin/reports/appointments")
                ?? new List<AppointmentReportDto>();
        }

        public async Task<DashboardDto> GetDashboardAsync()
        {
            await AttachAuthHeaderAsync();

            return await _http.GetFromJsonAsync<DashboardDto>(
                       "api/admin/dashboard")
                   ?? new DashboardDto();
        }
    }
}