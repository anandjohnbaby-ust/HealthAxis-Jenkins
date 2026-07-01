using HealthAxis.API.Models;
using HealthAxis.Shared.Common;
using HealthAxis.Shared.DTOs.DoctorDtos;
using HealthAxis.Shared.Enums;

namespace HealthAxis.API.Repositories.Interfaces
{
    public interface IDoctorRepository : IRepository<Doctor>
    {

        Task<IEnumerable<Doctor>> GetAvailableDoctorsAsync(
            Specialisation? specialisation,
            string? search,
            CancellationToken ct = default);

        Task<PagedResult<Doctor>> GetDoctorsAsync(
            PaginationRequest request,
            Specialisation? specialisation,
            string? search,
            CancellationToken ct = default);

        Task<Doctor?> GetByUserIdAsync(
            string userId,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<Appointment>> GetAppointmentsAsync(
            int doctorId,
            CancellationToken ct = default);

        Task<IEnumerable<Appointment>> GetTodaysAppointmentsAsync(
            int doctorId,
            DateTime today,
            CancellationToken ct = default);

        Task<IEnumerable<Appointment>> GetWeeklyAppointmentsAsync(
            int doctorId,
            DateTime startDate,
            DateTime endDate,
            CancellationToken ct = default);

        Task<DoctorDashboardDto?> GetDashboardAsync(
            int doctorId,
            CancellationToken ct = default);
    }
}
