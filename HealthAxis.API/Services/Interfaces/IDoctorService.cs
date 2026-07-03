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
            CancellationToken ct = default);

        Task<DoctorDto> CreateDoctor(CreateDoctorDto dto);

        Task<DoctorDto> UpdateDoctor(
            int doctorId,
            UpdateDoctorDto dto);

        Task<IEnumerable<DoctorDto>> GetAvailableDoctorsAsync(
            Specialisation? specialisation,
            string? search,
            CancellationToken ct = default);

        // Doctor Portal
        Task<IEnumerable<AppointmentDto>> GetAppointmentsAsync(
            string userId,
            CancellationToken ct = default);

        Task<IEnumerable<AppointmentDto>> GetTodaysAppointmentsAsync(
            string userId,
            CancellationToken ct = default);

        Task<IEnumerable<AppointmentDto>> GetWeeklyAppointmentsAsync(
            string userId,
            CancellationToken ct = default);

        Task<AppointmentDto> UpdateAppointmentStatusAsync(
            int appointmentId,
            UpdateAppointmentStatusDto dto,
            CancellationToken ct = default);

        Task<HealthRecordDto> AddHealthRecordAsync(
            CreateHealthRecordDto dto,
            CancellationToken ct = default);

        Task<HealthRecordDto> GetHealthRecordByIdAsync(
            int id,
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