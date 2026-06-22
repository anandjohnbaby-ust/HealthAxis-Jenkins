using HealthAxis.Shared.DTOs.AppointmentDtos;
using HealthAxis.Shared.DTOs.DoctorDtos;
using HealthAxis.API.Models;
using HealthAxis.Shared.DTOs.AdminDtos;

namespace HealthAxis.API.Services.Interfaces
{
    public interface IAdminService
    {
        Task<IEnumerable<DoctorDto>> GetDoctors();

        Task<DoctorDto> CreateDoctor(CreateDoctorDto dto);

        Task<DoctorDto> UpdateDoctor(int doctorId, UpdateDoctorDto dto);
        
        Task<IEnumerable<AppointmentDto>> GetAppointmentReport();
    }

}
