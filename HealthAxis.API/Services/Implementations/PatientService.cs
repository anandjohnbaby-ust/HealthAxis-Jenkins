using AutoMapper;
using HealthAxis.API.Exceptions;
using HealthAxis.API.Repositories.Interfaces;
using HealthAxis.API.Services.Interfaces;
using HealthAxis.Shared.Common;
using HealthAxis.Shared.DTOs.AppointmentDtos;
using HealthAxis.Shared.DTOs.HealthRecordDtos;
using HealthAxis.Shared.DTOs.PatientDtos;

namespace HealthAxis.API.Services.Implementations
{
    public class PatientService : IPatientService
    {

        private const string PatientNotFoundMessage = "Patient not found.";

        #region Dependency Injection

        private readonly IPatientRepository _patientRepository;
        private readonly IMapper _mapper;

        public PatientService(
            IPatientRepository patientRepository,
            IMapper mapper)
        {
            _patientRepository = patientRepository;

            _mapper = mapper;
        }

        #endregion

        // Get Patient by Id
        public async Task<PatientDto?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var patient = await _patientRepository.GetByIdAsync(id, ct);

            if (patient == null)
            {
                throw new NotFoundException(PatientNotFoundMessage);
            }

            return _mapper.Map<PatientDto>(patient);
        }

        // Get All Patient (Search)
        public async Task<PagedResult<PatientDto>> GetPatientsAsync(
            PaginationRequest request,
            string? search,
            CancellationToken ct = default)
        {
            var pagedPatients = await _patientRepository.GetPatientsAsync(
                request,
                search,
                ct);

            return new PagedResult<PatientDto>
            {
                Items = _mapper.Map<IEnumerable<PatientDto>>(pagedPatients.Items),
                TotalCount = pagedPatients.TotalCount,
                PageNumber = pagedPatients.PageNumber,
                PageSize = pagedPatients.PageSize
            };
        }

        // Update Patient
        public async Task<PatientDto> UpdateAsync(
                int id,
                UpdatePatientDto dto,
                CancellationToken ct = default)
        {
            var existingPatient =
                await _patientRepository.GetByIdAsync(id, ct);

            if (existingPatient is null)
            {
                throw new NotFoundException(PatientNotFoundMessage);
            }

            _mapper.Map(dto, existingPatient);

            var updatedPatient =
                await _patientRepository.UpdateAsync(id, existingPatient, ct);

            return _mapper.Map<PatientDto>(updatedPatient);
        }

        public async Task<PagedResult<HealthRecordDto>> GetHealthRecordsByPatientId(
            int patientId,
            PaginationRequest request,
            CancellationToken ct = default)
        {
            var records = await _patientRepository
                .GetHealthRecordsByPatientIdAsync(
                    patientId,
                    request,
                    ct);

            return new PagedResult<HealthRecordDto>
            {
                Items = _mapper.Map<List<HealthRecordDto>>(records.Items),
                TotalCount = records.TotalCount,
                PageNumber = records.PageNumber,
                PageSize = records.PageSize
            };
        }

        public async Task<PagedResult<AppointmentDto>> GetAppointmentsByPatientIdAsync(
            int patientId,
            PaginationRequest request,
            CancellationToken ct = default)
        {
            var patient = await _patientRepository.GetByIdAsync(
                patientId,
                ct);

            if (patient is null)
            {
                throw new NotFoundException(PatientNotFoundMessage);
            }

            var appointments = await _patientRepository.GetAppointmentsByPatientIdAsync(
                patientId,
                request,
                ct);

            return new PagedResult<AppointmentDto>
            {
                Items = _mapper.Map<List<AppointmentDto>>(appointments.Items),
                TotalCount = appointments.TotalCount,
                PageNumber = appointments.PageNumber,
                PageSize = appointments.PageSize
            };
        }

        public async Task<PatientDashboardDto> GetDashboardAsync(
            int patientId,
            CancellationToken ct = default)
        {
            var dashboard = await _patientRepository.GetDashboardAsync(
                patientId,
                ct);

            if (dashboard is null)
            {
                throw new NotFoundException(PatientNotFoundMessage);
            }

            return dashboard;
        }
    }
}
