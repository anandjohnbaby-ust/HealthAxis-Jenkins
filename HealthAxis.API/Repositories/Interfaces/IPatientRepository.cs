using HealthAxis.API.Models;
using HealthAxis.Shared.Common;
using HealthAxis.Shared.DTOs.PatientDtos;

namespace HealthAxis.API.Repositories.Interfaces
{
    public interface IPatientRepository : IRepository<Patient>
    {

        Task<Patient?> GetByUserIdAsync(
        string userId,
        CancellationToken cancellationToken = default);

        Task<Patient?> GetHealthRecordsByPatientId(int patientId,
        CancellationToken cancellationToken = default);

        Task<PagedResult<Patient>> GetPatientsAsync(
            PaginationRequest request,
            string? search,
            CancellationToken ct = default);

        Task<IEnumerable<Appointment>> GetAppointmentsByPatientIdAsync(
            int patientId,
            CancellationToken ct = default);


        Task<PatientDashboardDto?> GetDashboardAsync(
            int patientId,
            CancellationToken ct = default);
    }
}