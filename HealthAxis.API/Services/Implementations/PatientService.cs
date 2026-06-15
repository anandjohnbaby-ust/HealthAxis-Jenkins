using AutoMapper;
using HealthAxis.API.DTOs.PatientDtos;
using HealthAxis.API.Exceptions;
using HealthAxis.API.Models;
using HealthAxis.API.Repositories.Implementations;
using HealthAxis.API.Repositories.Interfaces;
using HealthAxis.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace HealthAxis.API.Services.Implementations
{
    public class PatientService : IPatientService
    {
        private readonly IRepository<Patient>
            _patientRepository;

        private readonly IRepository<Appointment>
            _appointmentRepository;

        private readonly IRepository<HealthRecord>
            _healthRecordRepository;

        private readonly IMapper _mapper;

        public PatientService(
            IRepository<Patient> patientRepository,
            IRepository<Appointment> appointmentRepository,
            IRepository<HealthRecord> healthRecordRepository,
            IMapper mapper)
        {
            _patientRepository =
                patientRepository;

            _appointmentRepository =
                appointmentRepository;

            _healthRecordRepository =
                healthRecordRepository;

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
                await _patientRepository.UpdateAsync(
                    id,
                    existingPatient,
                    ct);

            return _mapper.Map<PatientDto>(updatedPatient);
        }

        public async Task<IEnumerable<HealthRecordDto>>
            GetHealthRecordsAsync(
                int patientId,
                CancellationToken ct = default)
        {
            var patient =
                await _patientRepository
                    .GetByIdAsync(
                        patientId,
                        ct);

            if (patient is null)
            {
                throw new NotFoundException("Patient not found.");
            }

            var appointments =
                await _appointmentRepository
                    .GetAllAsync(ct);

            var appointmentIds =
                appointments
                    .Where(a =>
                        a.PatientId == patientId)
                    .Select(a =>
                        a.AppointmentId)
                    .ToHashSet();

            var healthRecords =
                await _healthRecordRepository
                    .GetAllAsync(ct);

            var patientHealthRecords =
                healthRecords
                    .Where(hr =>
                        appointmentIds.Contains(
                            hr.AppointmentId));

            return _mapper.Map<
                IEnumerable<HealthRecordDto>>(
                    patientHealthRecords);
        }
    }
}
