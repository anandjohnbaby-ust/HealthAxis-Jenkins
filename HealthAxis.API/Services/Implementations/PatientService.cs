using AutoMapper;
using HealthAxis.Shared.DTOs.HealthRecordDtos;
using HealthAxis.Shared.DTOs.PatientDtos;
using HealthAxis.API.Exceptions;
using HealthAxis.API.Models;
using HealthAxis.API.Repositories.Interfaces;
using HealthAxis.API.Services.Interfaces;

namespace HealthAxis.API.Services.Implementations
{
    public class PatientService : IPatientService
    {
        private readonly IPatientRepository
            _patientRepository;

        private readonly IMapper _mapper;

        public PatientService(
            IPatientRepository patientRepository,
            IRepository<Appointment> appointmentRepository,
            IRepository<HealthRecord> healthRecordRepository,
            IMapper mapper)
        {
            _patientRepository =
                patientRepository;

            _mapper = mapper;
        }

        public async Task<IEnumerable<PatientDto>> GetAllAsync(CancellationToken ct = default)
        {
            var patients = await _patientRepository.GetAllAsync(ct);

            return _mapper.Map<IEnumerable<PatientDto>>(patients);
        }

        public async Task<PatientDto?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var patient = await _patientRepository.GetByIdAsync(id, ct);

            if (patient == null)
            {
                throw new NotFoundException("Patient not found.");
            }

            return _mapper.Map<PatientDto>(patient);
        }

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
    }
}
