using HealthAxis.API.Models;
using HealthAxis.Shared.Common;
using HealthAxis.Shared.DTOs.AppointmentDtos;
using HealthAxis.Shared.DTOs.DoctorDtos;
using HealthAxis.Shared.Enums;

namespace HealthAxis.API.Repositories.Interfaces
{
    public interface IDoctorRepository : IRepository<Doctor>
    {

        Task<Doctor> CreateDoctorAsync(
            Doctor doctor,
            CancellationToken ct = default);

        Task<PagedResult<Doctor>> GetAvailableDoctorsAsync(
            Specialisation? specialisation,
            string? search,
            PaginationRequest request,
            CancellationToken ct = default);

        Task<PagedResult<Doctor>> GetDoctorsAsync(
            PaginationRequest request,
            Specialisation? specialisation,
            string? search,
            bool? isActive,
            CancellationToken ct = default);

        Task<Doctor?> GetByUserIdAsync(
            string userId,
            CancellationToken cancellationToken = default);

        Task<PagedResult<Appointment>> GetAppointmentsAsync(
                 int doctorId,
                 PaginationRequest request,
                 string? search = null,
                 AppointmentStatus? status = null,
                 DateTime? date = null,
                 CancellationToken ct = default);
        Task<PagedResult<Appointment>> GetTodaysAppointmentsAsync(
                    int doctorId,
                    DateTime today,
                    PaginationRequest request,
                    string? search = null,
                    AppointmentStatus? status = null,
                    CancellationToken ct = default);

        Task<PagedResult<Appointment>> GetWeeklyAppointmentsAsync(
                    int doctorId,
                    PaginationRequest request,
                    string? search = null,
                    AppointmentStatus? status = null,
                    DateTime? date = null,
                    CancellationToken ct = default);
        Task<DoctorDashboardDto?> GetDashboardAsync(
            int doctorId,
            CancellationToken ct = default);
    }
}
