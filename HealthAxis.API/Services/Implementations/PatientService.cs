using AutoMapper;
using HealthAxis.API.Exceptions;
using HealthAxis.API.Models;
using HealthAxis.API.Repositories.Interfaces;
using HealthAxis.API.Services.Interfaces;
using HealthAxis.Shared.Common;
using HealthAxis.Shared.DTOs.AppointmentDtos;
using HealthAxis.Shared.DTOs.DoctorDtos;
using HealthAxis.Shared.DTOs.HealthRecordDtos;
using HealthAxis.Shared.DTOs.PatientDtos;
using HealthAxis.Shared.Enums;

namespace HealthAxis.API.Services.Implementations
{
    public class PatientService : IPatientService
    {

        #region Dependency Injection

        private readonly IPatientRepository _patientRepository;
        private readonly IAppointmentService _appointmentService;
        private readonly IDoctorService _doctorService;
        private readonly IMapper _mapper;

        public PatientService(
            IPatientRepository patientRepository,
            IAppointmentService appointmentService,
            IDoctorService doctorService,
            IMapper mapper)
        {
            _patientRepository =
                patientRepository;

            _appointmentService = appointmentService;

            _doctorService = doctorService;

            _mapper = mapper;
        }

        #endregion

        // Get Patient by Id
        public async Task<PatientDto?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var patient = await _patientRepository.GetByIdAsync(id, ct);

            if (patient == null)
            {
                throw new NotFoundException("Patient not found.");
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
                throw new NotFoundException("Patient not found.");
            }

            _mapper.Map(dto, existingPatient);

            var updatedPatient =
                await _patientRepository.UpdateAsync(id, existingPatient, ct);

            return _mapper.Map<PatientDto>(updatedPatient);
        }

        public async Task<IEnumerable<HealthRecordDto>> GetHealthRecordsByPatientId(int patientId, CancellationToken ct = default)
        {
            var patient =
                await _patientRepository
                    .GetHealthRecordsByPatientId(patientId);

            if (patient is null)
            {
                throw new NotFoundException(
                    $"Patient with ID {patientId} not found");
            }

            return _mapper.Map<IEnumerable<HealthRecordDto>>
                (patient.HealthRecords);
        }


        public async Task<AppointmentDto> BookAppointmentAsync(
            CreateAppointmentDto dto,
            CancellationToken ct = default)
        {
            return await _appointmentService.BookAppointmentAsync(dto, ct);
        }

        public async Task<IEnumerable<DoctorDto>> GetAvailableDoctorsAsync(
            Specialisation? specialisation,
            string? search,
            CancellationToken ct = default)
        {
            return await _doctorService.GetAvailableDoctorsAsync(
                specialisation,
                search,
                ct);
        }

        public async Task<IEnumerable<AppointmentDto>> GetAppointmentsByPatientIdAsync(
            int patientId,
            CancellationToken ct = default)
        {
            var patient = await _patientRepository.GetByIdAsync(patientId, ct);

            if (patient is null)
            {
                throw new NotFoundException("Patient not found.");
            }

            var appointments =
                await _patientRepository.GetAppointmentsByPatientIdAsync(
                    patientId,
                    ct);

            return _mapper.Map<IEnumerable<AppointmentDto>>(appointments);
        }

        public async Task<AppointmentDto> CancelAppointmentByPatientAsync(
            int patientId,
            int appointmentId,
            CancelAppointmentDto dto,
            CancellationToken ct = default)
        {
            return await _appointmentService.CancelAppointmentByPatientAsync(
                patientId,
                appointmentId,
                dto,
                ct);
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
                throw new NotFoundException("Patient not found.");
            }

            return dashboard;
        }
    }
}
