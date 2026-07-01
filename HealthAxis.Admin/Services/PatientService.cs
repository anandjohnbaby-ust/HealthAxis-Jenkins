using HealthAxis.Shared.Common;
using HealthAxis.Shared.DTOs.PatientDtos;
using Microsoft.JSInterop;
using System.Net.Http.Headers;
using System.Net.Http.Json;

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

        public async Task<PagedResult<PatientDto>?> GetPatients(
            int pageNumber = 1,
            int pageSize = 10,
            string? search = null)
        {
            await SetAuthorizationHeader();

            var url = "api/admin/patients";

            var query = new List<string>
            {
                $"pageNumber={pageNumber}",
                $"pageSize={pageSize}"
            };

            if (!string.IsNullOrWhiteSpace(search))
            {
                query.Add($"search={Uri.EscapeDataString(search)}");
            }

            url += "?" + string.Join("&", query);

            return await _http.GetFromJsonAsync<PagedResult<PatientDto>>(url);
        }

        public async Task<PatientDto> UpdatePatient(
            int id,
            UpdatePatientDto dto)
        {
            await SetAuthorizationHeader();

            var response = await _http.PutAsJsonAsync(
                $"api/admin/patients/{id}",
                dto);

            response.EnsureSuccessStatusCode();

            return (await response.Content.ReadFromJsonAsync<PatientDto>())!;
        }


    }
}