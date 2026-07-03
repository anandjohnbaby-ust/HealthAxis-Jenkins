using HealthAxis.Shared.Common;
using HealthAxis.Shared.DTOs.PatientDtos;
using Microsoft.JSInterop;
using System.Net.Http.Json;

namespace HealthAxis.Admin.Services
{
    public class PatientService : AuthorizedApiServiceBase
    {
        public PatientService(HttpClient http, IJSRuntime js) : base(http, js)
        {
        }

        public async Task<PagedResult<PatientDto>?> GetPatients(
            int pageNumber = 1,
            int pageSize = 10,
            string? search = null)
        {
            await AttachAuthHeaderAsync();

            var query = new List<string>
            {
                $"pageNumber={pageNumber}",
                $"pageSize={pageSize}"
            };

            if (!string.IsNullOrWhiteSpace(search))
            {
                query.Add($"search={Uri.EscapeDataString(search)}");
            }

            var url = "api/admin/patients?" + string.Join("&", query);

            return await Http.GetFromJsonAsync<PagedResult<PatientDto>>(url);
        }

        public async Task<PatientDto> UpdatePatient(int id, UpdatePatientDto dto)
        {
            await AttachAuthHeaderAsync();

            var response = await Http.PutAsJsonAsync($"api/admin/patients/{id}", dto);
            response.EnsureSuccessStatusCode();

            return (await response.Content.ReadFromJsonAsync<PatientDto>())!;
        }
    }
}