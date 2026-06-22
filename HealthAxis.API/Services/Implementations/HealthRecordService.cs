using AutoMapper;
using HealthAxis.Shared.DTOs.HealthRecordDtos;
using HealthAxis.Shared.Enums;
using HealthAxis.API.Exceptions;
using HealthAxis.API.Models;
using HealthAxis.API.Repositories.Interfaces;
using HealthAxis.API.Services.Interfaces;

namespace HealthAxis.API.Services.Implementations
{
    public class HealthRecordService :
        IHealthRecordService
    {
        private readonly IHealthRecordRepository
            _healthRecordRepository;

        private readonly IAppointmentRepository
            _appointmentRepository;

        private readonly IMapper _mapper;

        public HealthRecordService(
            IHealthRecordRepository healthRecordRepository,
            IAppointmentRepository appointmentRepository,
            IMapper mapper)
        {
            _healthRecordRepository =
                healthRecordRepository;

            _appointmentRepository =
                appointmentRepository;

            _mapper = mapper;
        }

        public async Task<HealthRecordDto>
            GetByRecordIdAsync(
                int id,
                CancellationToken ct = default)
        {
            var healthRecord =
                await _healthRecordRepository
                    .GetByIdAsync(id, ct);

            if (healthRecord is null)
            {
                throw new NotFoundException(
                    "Health record not found.");
            }

            return _mapper.Map<HealthRecordDto>(
                healthRecord);
        }

        public async Task<HealthRecordDto> AddAsync(
            CreateHealthRecordDto dto,
            CancellationToken ct = default)
        {
            var appointment =
                await _appointmentRepository.GetByIdAsync(
                    dto.AppointmentId,
                    ct);

            if (appointment is null)
            {
                throw new NotFoundException(
                    "Appointment not found.");
            }

            if (appointment.Status !=
                AppointmentStatus.Completed)
            {
                throw new ValidationException(
                    "Health records can only be created for completed appointments.");
            }

            var records =
                await _healthRecordRepository.GetAllAsync(ct);

            bool exists =
                records.Any(hr =>
                    hr.AppointmentId ==
                    dto.AppointmentId);

            if (exists)
            {
                throw new ValidationException(
                    "Health record already exists for this appointment.");
            }

            var healthRecord = new HealthRecord
            {
                AppointmentId = dto.AppointmentId,

                DoctorId = appointment.DoctorId,

                PatientId = appointment.PatientId,

                VisitDate = appointment.ScheduledDate,

                Diagnosis = dto.Diagnosis,

                Prescription = dto.Prescription,

                Notes = dto.Notes
            };

            var savedRecord =
                await _healthRecordRepository.AddAsync(
                    healthRecord,
                    ct);

            return _mapper.Map<HealthRecordDto>(
                savedRecord);
        }

        public async Task<IEnumerable<HealthRecordDto>> GetByPatientIdAsync(
            int patientId,
            CancellationToken ct = default)
        {
            var records =
                await _healthRecordRepository.GetByPatientIdAsync(
                    patientId,
                    ct);

            if (!records.Any())
            {
                throw new NotFoundException(
                    $"No health records found for patient with ID {patientId}.");
            }

            return _mapper.Map<IEnumerable<HealthRecordDto>>(
                records);
        }
    }
}