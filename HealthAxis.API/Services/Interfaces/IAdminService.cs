using HealthAxis.API.Models;
using HealthAxis.Shared.Common;
using HealthAxis.Shared.DTOs.AdminDtos;
using HealthAxis.Shared.DTOs.AppointmentDtos;
using HealthAxis.Shared.DTOs.DoctorDtos;
using HealthAxis.Shared.DTOs.PatientDtos;
using HealthAxis.Shared.Enums;

namespace HealthAxis.API.Services.Interfaces
{
    public interface IAdminService
    {

        #region Doctor

        Task<DoctorDto> GetDoctorById(int id);

        Task<PagedResult<DoctorDto>> GetDoctorsAsync(
            PaginationRequest request,
            Specialisation? specialisation,
            string? search,
            CancellationToken ct = default);

        Task<DoctorDto> CreateDoctor(CreateDoctorDto dto);

        Task<DoctorDto> UpdateDoctor(int doctorId, UpdateDoctorDto dto);

        #endregion

        #region Patient
        Task<PagedResult<PatientDto>> GetPatientsAsync(
            PaginationRequest request,
            string? search,
            CancellationToken ct = default);

        Task<PatientDto> UpdatePatientAsync(
            int id,
            UpdatePatientDto dto,
            CancellationToken ct = default);

        #endregion

        Task<PagedResult<AppointmentReportDto>> GetAppointmentReport(PaginationRequest request);

        Task<DashboardDto> GetDashboardAsync();




    }
}
