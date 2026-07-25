using AutoMapper;
using HealthAxis.API.Events;
using HealthAxis.API.Exceptions;
using HealthAxis.API.Models;
using HealthAxis.API.Repositories.Interfaces;
using HealthAxis.API.Services.Interfaces;
using HealthAxis.Shared.DTOs.AppointmentDtos;
using HealthAxis.Shared.Enums;
using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using System.Globalization;
using System.Text.Json;

namespace HealthAxis.API.Services.Implementations
{
    public partial class AppointmentService : IAppointmentService
    {
        private const string DateFormat = "yyyy-MM-dd";
        private const string AppointmentNotFound = "Appointment not found.";

        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IRepository<Doctor> _doctorRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IMapper _mapper;
        private readonly IPublishEndpoint _publishEndPoint;
        //private readonly IDistributedCache _cache;
        private readonly ILogger<AppointmentService> _logger;

        private static readonly List<TimeSlotDto> AllTimeSlots =
        [
            new() { Value = "09:00:00", Label = "09:00 AM - 10:00 AM" },
            new() { Value = "10:00:00", Label = "10:00 AM - 11:00 AM" },
            new() { Value = "11:00:00", Label = "11:00 AM - 12:00 PM" },
            new() { Value = "12:00:00", Label = "12:00 PM - 01:00 PM" },
            new() { Value = "13:00:00", Label = "01:00 PM - 02:00 PM" },
            new() { Value = "14:00:00", Label = "02:00 PM - 03:00 PM" },
            new() { Value = "15:00:00", Label = "03:00 PM - 04:00 PM" },
            new() { Value = "16:00:00", Label = "04:00 PM - 05:00 PM" },
            new() { Value = "17:00:00", Label = "05:00 PM - 06:00 PM" }
        ];

        public AppointmentService(
            IAppointmentRepository appointmentRepository,
            IRepository<Doctor> doctorRepository,
            IPatientRepository patientRepository,
            IMapper mapper,
            IPublishEndpoint publishEndPoint,
            //IDistributedCache cache,
            ILogger<AppointmentService> logger)
        {
            _appointmentRepository = appointmentRepository;
            _doctorRepository = doctorRepository;
            _patientRepository = patientRepository;
            _publishEndPoint = publishEndPoint;
            _mapper = mapper;
            //_cache = cache;
            _logger = logger;
        }

        [LoggerMessage(
            EventId = 1,
            Level = LogLevel.Information,
            Message =
                """

        ┌──────────────────────────────────────────────────────────┐
        │                  HEALTHAXIS APPOINTMENT EVENT            │
        ├──────────────────────────────────────────────────────────┤
        │ Event Type      : Appointment Created
        │ Patient Name    : {PatientName}
        │ Doctor Name     : {DoctorName}
        │ Doctor ID       : {DoctorId}
        │ Appointment ID  : {AppointmentId}
        │ Scheduled Date  : {ScheduledDate:yyyy-MM-dd}
        │ Time Slot       : {TimeSlot}
        │ Status          : {Status}
        └──────────────────────────────────────────────────────────┘

        """)]
        private partial void LogAppointmentCreated(
            int appointmentId,
            int doctorId,
            string patientName,
            string doctorName,
            DateTime scheduledDate,
            TimeOnly timeSlot,
            string status);

        public async Task<IEnumerable<AppointmentDto>> GetAllAsync(
            CancellationToken ct = default)
        {
            var appointments =
                await _appointmentRepository.GetAllAsync(ct);

            return _mapper.Map<IEnumerable<AppointmentDto>>(
                appointments);
        }

        public async Task<List<TimeSlotDto>> GetAvailableSlotsAsync(
           int doctorId,
           DateTime date,
           CancellationToken ct = default)
        {

            var bookedSlots = await _appointmentRepository.GetBookedTimeSlotsAsync(
                doctorId,
                date,
                ct);

            var availableSlots = AllTimeSlots
                .Where(slot =>
                    !bookedSlots.Contains(
                        TimeOnly.Parse(
                            slot.Value,
                            CultureInfo.InvariantCulture)))
                .ToList();

            return availableSlots;
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

            // Check whether the patient has already booked
            // two appointments with the same doctor on the same day
            var appointmentCount =
                await _appointmentRepository.GetPatientDoctorAppointmentCountAsync(
                    dto.PatientId,
                    dto.DoctorId,
                    dto.ScheduledDate,
                    ct);

            if (appointmentCount >= 2)
            {
                throw new ValidationException(
                    "You cannot book more than two appointments with the same doctor on the same day.");
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

            await _publishEndPoint.Publish(new AppointmentEvent
            {
                EventType = "AppointmentCreated",
                AppointmentId = savedAppointment.AppointmentId,
                PatientId = savedAppointment.PatientId,
                DoctorId = savedAppointment.DoctorId,
                OccurredAt = DateTime.UtcNow
            }, ct);

            LogAppointmentCreated(
                savedAppointment.AppointmentId,
                savedAppointment.DoctorId,
                patient.FullName,
                doctor.FullName,
                savedAppointment.ScheduledDate,
                savedAppointment.TimeSlot,
                savedAppointment.Status.ToString());

            return _mapper.Map<AppointmentDto>(
                savedAppointment);
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
                    AppointmentNotFound);
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
                await _appointmentRepository.DeleteAsync(id, ct)
                ?? throw new NotFoundException(AppointmentNotFound);

            await _publishEndPoint.Publish(new AppointmentEvent
            {
                EventType = "AppointmentDeleted",
                AppointmentId = deletedAppointment.AppointmentId,
                PatientId = deletedAppointment.PatientId,
                DoctorId = deletedAppointment.DoctorId,
                OccurredAt = DateTime.UtcNow
            }, ct);

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
                throw new NotFoundException(AppointmentNotFound);
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
                ct)
                ?? throw new NotFoundException(AppointmentNotFound);

            await _publishEndPoint.Publish(new AppointmentEvent
            {
                EventType = "PatientCancelled",
                AppointmentId = updatedAppointment.AppointmentId,
                PatientId = updatedAppointment.PatientId,
                DoctorId = updatedAppointment.DoctorId,
                OccurredAt = DateTime.UtcNow
            }, ct);

            return _mapper.Map<AppointmentDto>(updatedAppointment);
        }
    }
}