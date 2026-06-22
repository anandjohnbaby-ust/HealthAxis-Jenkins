using AutoMapper;
using HealthAxis.Shared.Enums;
using HealthAxis.API.Models;
using HealthAxis.API.Repositories.Interfaces;
using HealthAxis.API.Services.Interfaces;
using HealthAxis.API.Exceptions;
using HealthAxis.Shared.DTOs.AppointmentDtos;

namespace HealthAxis.API.Services.Implementations
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IRepository<Appointment> _appointmentRepository;
        private readonly IRepository<Doctor> _doctorRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IMapper _mapper;

        public AppointmentService(
            IRepository<Appointment> appointmentRepository,
            IRepository<Doctor> doctorRepository,
            IPatientRepository patientRepository,
            IMapper mapper)
        {
            _appointmentRepository = appointmentRepository;
            _doctorRepository = doctorRepository;
            _patientRepository = patientRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<AppointmentDto>> GetAllAsync(
            CancellationToken ct = default)
        {
            var appointments =
                await _appointmentRepository.GetAllAsync(ct);

            return _mapper.Map<IEnumerable<AppointmentDto>>(
                appointments);
        }

        // Add Appointment
        public async Task<AppointmentDto> AddAsync(
            CreateAppointmentDto dto,
            CancellationToken ct = default)
        {
            if (dto.ScheduledDate.Date < DateTime.Today)
            {
                throw new ValidationException(
                    "Appointments cannot be booked for past dates.");
            }

            if (dto.ScheduledDate.Date >
                DateTime.Today.AddMonths(6))
            {
                throw new ValidationException(
                    "Appointments can only be booked up to 6 months in advance.");
            }

            var patient =
                await _patientRepository.GetByIdAsync(dto.PatientId, ct);

            if (patient is null)
            {
                throw new NotFoundException(
                    "Patient not found.");
            }

            var doctor =
                await _doctorRepository.GetByIdAsync(
                    dto.DoctorId,
                    ct);

            if (doctor is null)
            {
                throw new NotFoundException(
                    "Doctor not found.");
            }

            if (!doctor.IsActive)
            {
                throw new ValidationException(
                    "Appointments cannot be booked with inactive doctors.");
            }

            /////////////////////////////////////////////
            // Do Validation for TimeSlot conflict also//
            /////////////////////////////////////////////
            
            var appointment =
                _mapper.Map<Appointment>(dto);

            appointment.Status =
                AppointmentStatus.Pending;

            var savedAppointment =
                await _appointmentRepository.AddAsync(appointment, ct);

            return _mapper.Map<AppointmentDto>(
                savedAppointment);
        }

        // Update Appointment Status
        public async Task<AppointmentDto> UpdateStatusAsync(
            int id,
            UpdateAppointmentStatusDto dto,
            CancellationToken ct = default)
        {
            var appointment =
                await _appointmentRepository.GetByIdAsync(id, ct);

            if (appointment is null)
            {
                throw new NotFoundException(
                    "Appointment not found.");
            }

            // Prevent changing status of cancelled appointments
            if (appointment.Status == AppointmentStatus.Cancelled)
            {
                throw new ValidationException(
                    "Cancelled appointments cannot be modified.");
            }

            // Prevent changing status of completed appointments
            if (appointment.Status == AppointmentStatus.Completed)
            {
                throw new ValidationException(
                    "Completed appointments cannot be modified.");
            }

            // Prevent updating to the same status
            if (appointment.Status == dto.Status)
            {
                throw new ValidationException(
                    $"Appointment is already {dto.Status}.");
            }

            // Prevent completing a pending appointment directly
            if (appointment.Status == AppointmentStatus.Pending &&
                dto.Status == AppointmentStatus.Completed)
            {
                throw new ValidationException(
                    "Pending appointments must be confirmed before completion.");
            }

            switch (dto.Status)
            {
                case AppointmentStatus.Confirmed:

                    appointment.Confirm();

                    break;

                case AppointmentStatus.Cancelled:

                    appointment.Cancel(
                        dto.CancellationReason ?? string.Empty);

                    break;

                case AppointmentStatus.Completed:

                    appointment.Complete();

                    break;

                case AppointmentStatus.Pending:

                    throw new ValidationException(
                        "Appointments cannot be reverted to pending status.");
            }

            var updatedAppointment =
                await _appointmentRepository.UpdateAsync(id, appointment, ct);

            return _mapper.Map<AppointmentDto>(
                updatedAppointment);
        }


        // Delete Appointment
        public async Task<AppointmentDto> DeleteAsync(
            int id,
            CancellationToken ct = default)
        {
            var appointment =
                await _appointmentRepository.GetByIdAsync(id, ct);

            if (appointment is null)
            {
                throw new NotFoundException(
                    "Appointment not found.");
            }

            // Prevent deleting completed appointments
            if (appointment.Status ==
                AppointmentStatus.Completed)
            {
                throw new ValidationException(
                    "Completed appointments cannot be deleted.");
            }

            // Prevent deleting confirmed appointments
            if (appointment.Status ==
                AppointmentStatus.Confirmed)
            {
                throw new ValidationException(
                    "Confirmed appointments cannot be deleted.");
            }

            var deletedAppointment =
                await _appointmentRepository.DeleteAsync(id, ct);

            return _mapper.Map<AppointmentDto>(
                deletedAppointment);
        }
    }
}