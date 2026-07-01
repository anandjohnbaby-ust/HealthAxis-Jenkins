using HealthAxis.Shared.Common;
using HealthAxis.Shared.DTOs.AdminDtos;
using Microsoft.JSInterop;
using System.Net.Http.Headers;
using System.Net.Http.Json;

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

        public async Task<PagedResult<AppointmentReportDto>?> GetAppointmentReportAsync(
            int pageNumber = 1,
            int pageSize = 10)
        {
            await AttachAuthHeaderAsync();

            var url = $"api/admin/reports/appointments?pageNumber={pageNumber}&pageSize={pageSize}";

            return await _http.GetFromJsonAsync<PagedResult<AppointmentReportDto>>(url);
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