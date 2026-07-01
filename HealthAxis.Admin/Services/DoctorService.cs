using HealthAxis.Shared.Common;
using HealthAxis.Shared.DTOs.AdminDtos;
using HealthAxis.Shared.DTOs.DoctorDtos;
using HealthAxis.Shared.Enums;
using Microsoft.JSInterop;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace HealthAxis.Admin.Services
{
    public class DoctorService
    {
        private readonly HttpClient _http;
        private readonly IJSRuntime _js;

        public DoctorService(HttpClient http, IJSRuntime js)
        {
            _http = http;
            _js = js;
        }

        private async Task SetAuthorizationHeader()
        {
            var token = await _js.InvokeAsync<string>(
                "localStorage.getItem",
                "token");

            if (!string.IsNullOrWhiteSpace(token))
            {
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }
        public async Task<PagedResult<DoctorDto>?> GetDoctors(
            int pageNumber = 1,
            int pageSize = 10,
            Specialisation? specialisation = null,
            string? search = null)
        {
            await SetAuthorizationHeader();

            var url = "api/admin/doctors";

            var query = new List<string>
            {
                $"pageNumber={pageNumber}",
                $"pageSize={pageSize}"
            };

            if (specialisation.HasValue)
                query.Add($"specialisation={specialisation}");

            if (!string.IsNullOrWhiteSpace(search))
                query.Add($"search={Uri.EscapeDataString(search)}");

            url += "?" + string.Join("&", query);

            return await _http.GetFromJsonAsync<PagedResult<DoctorDto>>(url);
        }

        public async Task<DoctorDto?> CreateDoctor(CreateDoctorDto dto)
        {
            await SetAuthorizationHeader();

            var response = await _http.PostAsJsonAsync(
                "api/admin/doctors",
                dto);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<DoctorDto>();
        }

        public async Task<DoctorDto?> UpdateDoctor(
            int id,
            UpdateDoctorDto dto)
        {
            await SetAuthorizationHeader();

            var response = await _http.PutAsJsonAsync(
                $"api/admin/doctors/{id}",
                dto);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<DoctorDto>();
        }
    }
}