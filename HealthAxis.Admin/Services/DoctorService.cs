using System.Net.Http.Headers;
using System.Net.Http.Json;
using HealthAxis.Shared.DTOs.AdminDtos;
using HealthAxis.Shared.DTOs.DoctorDtos;
using Microsoft.JSInterop;

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

        public async Task<List<DoctorDto>?> GetDoctors()
        {
            await SetAuthorizationHeader();

            return await _http.GetFromJsonAsync<List<DoctorDto>>(
                "api/admin/doctors");
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