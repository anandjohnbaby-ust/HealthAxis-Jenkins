using HealthAxis.Shared.Common;
using HealthAxis.Shared.DTOs.AdminDtos;
using HealthAxis.Shared.DTOs.DoctorDtos;
using HealthAxis.Shared.Enums;
using System.Net.Http.Json;

namespace HealthAxis.Admin.Services
{
    public class DoctorService
    {
        private readonly HttpClient _http;

        public DoctorService(HttpClient http)
        {
            _http = http;
        }

        public async Task<PagedResult<DoctorDto>?> GetDoctors(
            int pageNumber = 1,
            int pageSize = 10,
            Specialisation? specialisation = null,
            string? search = null,
            bool? isActive = null)
        {
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

            if (isActive.HasValue)
                query.Add($"isActive={isActive.Value.ToString().ToLower()}");

            url += "?" + string.Join("&", query);

            return await _http.GetFromJsonAsync<PagedResult<DoctorDto>>(url);
        }

        public async Task<DoctorDto?> CreateDoctor(CreateDoctorDto dto)
        {
            var response = await _http.PostAsJsonAsync("api/admin/doctors", dto);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<DoctorDto>();
        }

        public async Task<DoctorDto?> UpdateDoctor(int id, UpdateDoctorDto dto)
        {
            var response = await _http.PutAsJsonAsync($"api/admin/doctors/{id}", dto);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<DoctorDto>();
        }
    }
}