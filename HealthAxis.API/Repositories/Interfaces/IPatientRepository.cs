using HealthAxis.API.Models;
using HealthAxis.Shared.Common;
using HealthAxis.Shared.DTOs.PatientDtos;
using HealthAxis.Shared.Enums;

namespace HealthAxis.API.Repositories.Interfaces
{
    public interface IPatientRepository : IRepository<Patient>
    {

        Task<Patient?> GetByUserIdAsync(
        string userId,
        CancellationToken cancellationToken = default);

        Task<PagedResult<HealthRecord>> GetHealthRecordsByPatientIdAsync(
            int patientId,
            PaginationRequest request,
            CancellationToken cancellationToken = default);

        Task<PagedResult<Patient>> GetPatientsAsync(
            PaginationRequest request,
            string? search,
            CancellationToken ct = default);

        Task<PagedResult<Appointment>> GetAppointmentsByPatientIdAsync(
            int patientId,
            PaginationRequest request,
            string? search,
            AppointmentStatus? status,
            DateTime? date,
            CancellationToken ct = default);


        Task<PatientDashboardDto?> GetDashboardAsync(
            int patientId,
            CancellationToken ct = default);
    }
}