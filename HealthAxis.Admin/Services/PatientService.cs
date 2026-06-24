using System.Net.Http.Headers;
using System.Net.Http.Json;
using HealthAxis.Shared.DTOs.PatientDtos;
using Microsoft.JSInterop;

namespace HealthAxis.Admin.Services
{
    public class PatientService
    {
        private readonly HttpClient _http;
        private readonly IJSRuntime _js;

        public PatientService(HttpClient http, IJSRuntime js)
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

        public async Task<List<PatientDto>?> GetPatients(
            string? search = null)
        {
            await SetAuthorizationHeader();

            var url = "api/patients";

            if (!string.IsNullOrWhiteSpace(search))
            {
                url += $"?search={Uri.EscapeDataString(search)}";
            }

            return await _http.GetFromJsonAsync<List<PatientDto>>(url);
        }

        public async Task<PatientDto?> GetPatientById(int id)
        {
            await SetAuthorizationHeader();

            return await _http.GetFromJsonAsync<PatientDto>(
                $"api/patients/{id}");
        }

        public async Task<PatientDto?> UpdatePatient(
            int id,
            UpdatePatientDto dto)
        {
            await SetAuthorizationHeader();

            var response = await _http.PutAsJsonAsync(
                $"api/patients/{id}",
                dto);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<PatientDto>();
        }
    }
}