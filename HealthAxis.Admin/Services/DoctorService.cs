using HealthAxis.Shared.Common;
using HealthAxis.Shared.DTOs.AdminDtos;
using HealthAxis.Shared.DTOs.DoctorDtos;
using HealthAxis.Shared.Enums;
using Microsoft.JSInterop;
using System.Net.Http.Json;

namespace HealthAxis.Admin.Services
{
    public class DoctorService : AuthorizedApiServiceBase
    {
        public DoctorService(HttpClient http, IJSRuntime js) : base(http, js)
        {
        }

        public async Task<PagedResult<DoctorDto>?> GetDoctors(
            int pageNumber = 1,
            int pageSize = 10,
            Specialisation? specialisation = null,
            string? search = null)
        {
            await AttachAuthHeaderAsync();

            var query = new List<string>
            {
                $"pageNumber={pageNumber}",
                $"pageSize={pageSize}"
            };

            if (specialisation.HasValue)
            {
                query.Add($"specialisation={specialisation}");
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query.Add($"search={Uri.EscapeDataString(search)}");
            }

            var url = "api/admin/doctors?" + string.Join("&", query);

            return await Http.GetFromJsonAsync<PagedResult<DoctorDto>>(url);
        }

        public async Task<DoctorDto?> CreateDoctor(CreateDoctorDto dto)
        {
            await AttachAuthHeaderAsync();

            var response = await Http.PostAsJsonAsync("api/admin/doctors", dto);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<DoctorDto>();
        }

        public async Task<DoctorDto?> UpdateDoctor(int id, UpdateDoctorDto dto)
        {
            await AttachAuthHeaderAsync();

            var response = await Http.PutAsJsonAsync($"api/admin/doctors/{id}", dto);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<DoctorDto>();
        }
    }
}