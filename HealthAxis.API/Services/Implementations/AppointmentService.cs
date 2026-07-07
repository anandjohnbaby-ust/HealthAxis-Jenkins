using AutoMapper;
using HealthAxis.API.Events;
using HealthAxis.API.Exceptions;
using HealthAxis.API.Messaging;
using HealthAxis.API.Models;
using HealthAxis.API.Repositories.Interfaces;
using HealthAxis.API.Services.Interfaces;
using HealthAxis.Shared.Common;
using HealthAxis.Shared.DTOs.AdminDtos;
using HealthAxis.Shared.DTOs.AppointmentDtos;
using HealthAxis.Shared.Enums;

namespace HealthAxis.API.Services.Implementations
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IRepository<Doctor> _doctorRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IMapper _mapper;
        private readonly RabbitMQPublisher _publisher;

        public AppointmentService(
            IAppointmentRepository appointmentRepository,
            IRepository<Doctor> doctorRepository,
            IPatientRepository patientRepository,
            IMapper mapper,
            RabbitMQPublisher publisher)
        {
            _appointmentRepository = appointmentRepository;
            _doctorRepository = doctorRepository;
            _patientRepository = patientRepository;
            _mapper = mapper;
            _publisher = publisher;
        }

        public async Task<IEnumerable<AppointmentDto>> GetAllAsync(
            CancellationToken ct = default)
        {
            var appointments =
                await _appointmentRepository.GetAllAsync(ct);

            return _mapper.Map<IEnumerable<AppointmentDto>>(
                appointments);
        }

        // Get Appointment Report
        public async Task<PagedResult<AppointmentReportDto>> GetAppointmentReportAsync(
            PaginationRequest request)
        {
            return await _appointmentRepository.GetAppointmentReportAsync(request);
        }

        // Add Appointment
        public async Task<AppointmentDto> BookAppointmentAsync(
            CreateAppointmentDto dto,
            CancellationToken ct = default)
        {
            if (dto.ScheduledDate.Date < DateTime.Today)
            {
                throw new ValidationException(
                    "Appointments cannot be booked for past dates.");
            }

            if (dto.ScheduledDate.Date > DateTime.Today.AddMonths(6))
            {
                throw new ValidationException(
                    "Appointments can only be booked up to 6 months in advance.");
            }

            var patient =
                await _patientRepository.GetByIdAsync(
                    dto.PatientId,
                    ct);

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

            // Check whether the doctor already has an appointment
            if (await _appointmentRepository.IsTimeSlotBookedAsync(
                    dto.DoctorId,
                    dto.ScheduledDate,
                    dto.TimeSlot,
                    ct))
            {
                throw new ValidationException(
                    "The selected time slot is already booked for this doctor.");
            }

            // Check whether the patient already has an appointment
            if (await _appointmentRepository.IsTimeSlotBookedAsync(
                    dto.PatientId,
                    dto.ScheduledDate,
                    dto.TimeSlot,
                    ct))
            {
                throw new ValidationException(
                    "You already have another appointment at the selected time.");
            }

            var appointment =
                _mapper.Map<Appointment>(dto);

            appointment.Status =
                AppointmentStatus.Pending;

            var savedAppointment =
                await _appointmentRepository.AddAsync(
                    appointment,
                    ct);

            await _publisher.PublishAsync(new AppointmentEvent
            {
                EventType = "AppointmentCreated",
                AppointmentId = savedAppointment.AppointmentId,
                PatientId = savedAppointment.PatientId,
                DoctorId = savedAppointment.DoctorId,
                OccurredAt = DateTime.UtcNow
            });

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

            await _publisher.PublishAsync(new AppointmentEvent
            {
                EventType = appointment.Status.ToString(),
                AppointmentId = updatedAppointment.AppointmentId,
                PatientId = updatedAppointment.PatientId,
                DoctorId = updatedAppointment.DoctorId,
                OccurredAt = DateTime.UtcNow
            });

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

            await _publisher.PublishAsync(new AppointmentEvent
            {
                EventType = "AppointmentDeleted",
                AppointmentId = deletedAppointment.AppointmentId,
                PatientId = deletedAppointment.PatientId,
                DoctorId = deletedAppointment.DoctorId,
                OccurredAt = DateTime.UtcNow
            });

            return _mapper.Map<AppointmentDto>(
                deletedAppointment);
        }

        public async Task<AppointmentDto> CancelAppointmentByPatientAsync(
            int patientId,
            int appointmentId,
            CancelAppointmentDto dto,
            CancellationToken ct = default)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(
                appointmentId,
                ct);

            if (appointment is null)
            {
                throw new NotFoundException("Appointment not found.");
            }

            if (appointment.PatientId != patientId)
            {
                throw new ValidationException(
                    "This appointment does not belong to the patient.");
            }

            if (appointment.Status != AppointmentStatus.Pending)
            {
                throw new ValidationException(
                    "Only pending appointments can be cancelled.");
            }

            appointment.Status = AppointmentStatus.Cancelled;
            appointment.CancellationReason = dto.CancellationReason;

            var updatedAppointment = await _appointmentRepository.UpdateAsync(
                appointmentId,
                appointment,
                ct);

            await _publisher.PublishAsync(new AppointmentEvent
            {
                EventType = "PatientCancelled",
                AppointmentId = updatedAppointment.AppointmentId,
                PatientId = updatedAppointment.PatientId,
                DoctorId = updatedAppointment.DoctorId,
                OccurredAt = DateTime.UtcNow
            });

            return _mapper.Map<AppointmentDto>(updatedAppointment);
        }

    }
}