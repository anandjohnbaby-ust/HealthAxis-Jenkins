using HealthAxis.API.DTOs.AppointmentDtos;
using HealthAxis.API.DTOs.DoctorDtos;
using HealthAxis.API.Models;

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
