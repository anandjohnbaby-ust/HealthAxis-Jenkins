using HealthAxis.API.Models;
using HealthAxis.Shared.Common;
using HealthAxis.Shared.DTOs.AdminDtos;
using HealthAxis.Shared.DTOs.AppointmentDtos;
using HealthAxis.Shared.DTOs.DoctorDtos;
using HealthAxis.Shared.DTOs.HealthRecordDtos;
using HealthAxis.Shared.Enums;

namespace HealthAxis.API.Services.Interfaces
{
    public interface IDoctorService
    {
        Task<DoctorDto> GetDoctorById(int id);

        Task<PagedResult<DoctorDto>> GetDoctorsAsync(
                    PaginationRequest request,
                    Specialisation? specialisation,
                    string? search,
                    bool? isActive,
                    CancellationToken ct = default);

        Task<DoctorDto> CreateDoctor(CreateDoctorDto dto);

        Task<DoctorDto> UpdateDoctor(
            int doctorId,
            UpdateDoctorDto dto);

        Task<PagedResult<DoctorDto>> GetAvailableDoctorsAsync(
            Specialisation? specialisation,
            string? search,
            PaginationRequest request,
            CancellationToken ct = default);

        Task<PagedResult<AppointmentDto>> GetAppointmentsAsync(
                    string userId,
                    PaginationRequest request,
                    string? search = null,
                    AppointmentStatus? status = null,
                    DateTime? date = null,
                    CancellationToken ct = default);

        Task<PagedResult<AppointmentDto>> GetTodaysAppointmentsAsync(
                   string userId,
                   PaginationRequest request,
                   string? search = null,
                   AppointmentStatus? status = null,
                   CancellationToken ct = default);

        Task<PagedResult<AppointmentDto>> GetWeeklyAppointmentsAsync(
             string userId,
             PaginationRequest request,
             string? search = null,
             AppointmentStatus? status = null,
             DateTime? date = null,
             CancellationToken ct = default);

        Task<DoctorDto> GetDoctorByUserIdAsync(
            string userId,
            CancellationToken ct = default);

        Task<DoctorDto> UpdateDoctorByUserIdAsync(
            string userId,
            UpdateDoctorDto dto,
            CancellationToken ct = default);

        Task<DoctorDashboardDto> GetDashboardAsync(
            int doctorId,
            CancellationToken ct = default);

        Task<IEnumerable<HealthRecordDto>> GetPatientHealthHistoryAsync(
            int patientId,
            int appointmentId,
            CancellationToken ct = default);
    }
}