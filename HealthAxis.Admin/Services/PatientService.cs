using HealthAxis.Shared.DTOs.PatientDtos;
using HealthAxis.Shared.Common;
using System.Net.Http.Json;

namespace HealthAxis.Admin.Services
{
    public class PatientService
    {
        private readonly HttpClient _http;

        public PatientService(HttpClient http)
        {
            _http = http;
        }

        public async Task<PagedResult<PatientDto>?> GetPatients(
            int pageNumber = 1,
            int pageSize = 10,
            string? search = null)
        {
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

        public async Task<PatientDto> UpdatePatient(int id, UpdatePatientDto dto)
        {
            var response = await _http.PutAsJsonAsync($"api/admin/patients/{id}", dto);
            response.EnsureSuccessStatusCode();

            return (await response.Content.ReadFromJsonAsync<PatientDto>())!;
        }
    }
}