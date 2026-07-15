using AutoMapper;
using FluentAssertions;
using HealthAxis.API.Events;
using HealthAxis.API.Exceptions;
using HealthAxis.API.Models;
using HealthAxis.API.Repositories.Interfaces;
using HealthAxis.API.Services.Implementations;
using HealthAxis.Shared.DTOs.AppointmentDtos;
using HealthAxis.Shared.Enums;
using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text;
using System.Text.Json;
using Xunit;

namespace HealthAxis.Tests.Services
{
    public class AppointmentServiceTests
    {
        protected readonly Mock<IAppointmentRepository> _appointmentRepository;
        protected readonly Mock<IRepository<Doctor>> _doctorRepository;
        protected readonly Mock<IPatientRepository> _patientRepository;
        protected readonly Mock<IMapper> _mapper;
        protected readonly Mock<IPublishEndpoint> _publishEndpoint;
        protected readonly Mock<IDistributedCache> _cache;
        protected readonly Mock<ILogger<AppointmentService>> _logger;
        protected readonly AppointmentService _service;

        public AppointmentServiceTests()
        {
            _appointmentRepository = new Mock<IAppointmentRepository>();
            _doctorRepository = new Mock<IRepository<Doctor>>();
            _patientRepository = new Mock<IPatientRepository>();
            _mapper = new Mock<IMapper>();
            _publishEndpoint = new Mock<IPublishEndpoint>();
            _cache = new Mock<IDistributedCache>();
            _logger = new Mock<ILogger<AppointmentService>>();

            _service = new AppointmentService(
                _appointmentRepository.Object,
                _doctorRepository.Object,
                _patientRepository.Object,
                _mapper.Object,
                _publishEndpoint.Object,
                _cache.Object,
                _logger.Object);
        }

        // Standard helper to verify ILogger.Log(...) calls made by [LoggerMessage] source-gen methods

        #region Constructor

        [Fact]
        public void Constructor_ShouldCreateInstance_WhenDependenciesAreProvided()
        {
            var service = new AppointmentService(
                _appointmentRepository.Object,
                _doctorRepository.Object,
                _patientRepository.Object,
                _mapper.Object,
                _publishEndpoint.Object,
                _cache.Object,
                _logger.Object);

            service.Should().NotBeNull();
        }

        #endregion

        #region GetAllAsync

        [Fact]
        public async Task GetAllAsync_ShouldReturnAppointments_WhenRepositoryReturnsData()
        {
            var appointments = new List<Appointment>
            {
                new() { AppointmentId = 1 },
                new() { AppointmentId = 2 }
            };

            var appointmentDtos = new List<AppointmentDto>
            {
                new() { AppointmentId = 1 },
                new() { AppointmentId = 2 }
            };

            _appointmentRepository
                .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointments);

            _mapper
                .Setup(x => x.Map<IEnumerable<AppointmentDto>>(appointments))
                .Returns(appointmentDtos);

            var result = await _service.GetAllAsync();

            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().BeEquivalentTo(appointmentDtos);

            _appointmentRepository.Verify(
                x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);

            _mapper.Verify(
                x => x.Map<IEnumerable<AppointmentDto>>(appointments), Times.Once);

            _appointmentRepository.VerifyNoOtherCalls();
            _mapper.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnEmptyCollection_WhenRepositoryReturnsEmptyList()
        {
            var appointments = new List<Appointment>();
            var appointmentDtos = new List<AppointmentDto>();

            _appointmentRepository
                .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointments);

            _mapper
                .Setup(x => x.Map<IEnumerable<AppointmentDto>>(appointments))
                .Returns(appointmentDtos);

            var result = await _service.GetAllAsync();

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllAsync_ShouldThrowException_WhenRepositoryThrowsException()
        {
            _appointmentRepository
                .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database Error"));

            Func<Task> act = async () => await _service.GetAllAsync();

            await act.Should().ThrowAsync<Exception>().WithMessage("Database Error");
        }

        #endregion

        #region GetAvailableSlotsAsync

        [Fact]
        public async Task GetAvailableSlotsAsync_ShouldReturnCachedSlots_WhenCacheHit()
        {
            var doctorId = 1;
            var date = DateTime.Today;

            var cachedSlots = new List<TimeSlotDto>
            {
                new() { Value = "09:00:00", Label = "09:00 AM - 10:00 AM" }
            };

            var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(cachedSlots));

            _cache
                .Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(bytes);

            var result = await _service.GetAvailableSlotsAsync(doctorId, date);

            result.Should().HaveCount(1);
            result[0].Value.Should().Be("09:00:00");

            _appointmentRepository.Verify(
                x => x.GetBookedTimeSlotsAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task GetAvailableSlotsAsync_ShouldReturnEmptyList_WhenCachedDataDeserializesToNull()
        {
            var doctorId = 1;
            var date = DateTime.Today;

            // "null" is valid JSON that deserializes to a null List<TimeSlotDto>
            var bytes = Encoding.UTF8.GetBytes("null");

            _cache
                .Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(bytes);

            var result = await _service.GetAvailableSlotsAsync(doctorId, date);

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAvailableSlotsAsync_ShouldFetchFromRepositoryAndCache_WhenCacheMiss()
        {
            var doctorId = 1;
            var date = DateTime.Today;

            _cache
                .Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((byte[]?)null);

            var bookedSlots = new List<TimeOnly> { new TimeOnly(9, 0, 0) };

            _appointmentRepository
                .Setup(x => x.GetBookedTimeSlotsAsync(doctorId, date, It.IsAny<CancellationToken>()))
                .ReturnsAsync(bookedSlots);

            _cache
                .Setup(x => x.SetAsync(
                    It.IsAny<string>(),
                    It.IsAny<byte[]>(),
                    It.IsAny<DistributedCacheEntryOptions>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await _service.GetAvailableSlotsAsync(doctorId, date);

            // 09:00 slot should be excluded since it's booked; 8 slots should remain
            result.Should().HaveCount(8);
            result.Should().NotContain(s => s.Value == "09:00:00");

            _cache.Verify(
                x => x.SetAsync(
                    It.IsAny<string>(),
                    It.IsAny<byte[]>(),
                    It.IsAny<DistributedCacheEntryOptions>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetAvailableSlotsAsync_ShouldReturnAllSlots_WhenNoBookedSlots()
        {
            var doctorId = 1;
            var date = DateTime.Today;

            _cache
                .Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((byte[]?)null);

            _appointmentRepository
                .Setup(x => x.GetBookedTimeSlotsAsync(doctorId, date, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<TimeOnly>());

            var result = await _service.GetAvailableSlotsAsync(doctorId, date);

            result.Should().HaveCount(9);
        }

        #endregion

        #region BookAppointmentAsync

        private static CreateAppointmentDto ValidBookingDto() => new()
        {
            PatientId = 1,
            DoctorId = 1,
            ScheduledDate = DateTime.Today.AddDays(1),
            TimeSlot = new TimeOnly(9, 0, 0)
        };

        [Fact]
        public async Task BookAppointmentAsync_ShouldThrowValidationException_WhenDateIsInThePast()
        {
            var dto = ValidBookingDto();
            dto.ScheduledDate = DateTime.Today.AddDays(-1);

            Func<Task> act = async () => await _service.BookAppointmentAsync(dto);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("Appointments cannot be booked for past dates.");
        }

        [Fact]
        public async Task BookAppointmentAsync_ShouldThrowValidationException_WhenDateIsMoreThanSixMonthsAhead()
        {
            var dto = ValidBookingDto();
            dto.ScheduledDate = DateTime.Today.AddMonths(7);

            Func<Task> act = async () => await _service.BookAppointmentAsync(dto);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("Appointments can only be booked up to 6 months in advance.");
        }

        [Fact]
        public async Task BookAppointmentAsync_ShouldThrowNotFoundException_WhenPatientDoesNotExist()
        {
            var dto = ValidBookingDto();

            _patientRepository
                .Setup(x => x.GetByIdAsync(dto.PatientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Patient?)null);

            Func<Task> act = async () => await _service.BookAppointmentAsync(dto);

            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Patient not found.");
        }

        [Fact]
        public async Task BookAppointmentAsync_ShouldThrowNotFoundException_WhenDoctorDoesNotExist()
        {
            var dto = ValidBookingDto();

            _patientRepository
                .Setup(x => x.GetByIdAsync(dto.PatientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Patient { PatientId = dto.PatientId });

            _doctorRepository
                .Setup(x => x.GetByIdAsync(dto.DoctorId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Doctor?)null);

            Func<Task> act = async () => await _service.BookAppointmentAsync(dto);

            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Doctor not found.");
        }

        [Fact]
        public async Task BookAppointmentAsync_ShouldThrowValidationException_WhenDoctorIsInactive()
        {
            var dto = ValidBookingDto();

            _patientRepository
                .Setup(x => x.GetByIdAsync(dto.PatientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Patient { PatientId = dto.PatientId });

            _doctorRepository
                .Setup(x => x.GetByIdAsync(dto.DoctorId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Doctor { DoctorId = dto.DoctorId, IsActive = false });

            Func<Task> act = async () => await _service.BookAppointmentAsync(dto);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("Appointments cannot be booked with inactive doctors.");
        }

        [Fact]
        public async Task BookAppointmentAsync_ShouldThrowValidationException_WhenDoctorSlotAlreadyBooked()
        {
            var dto = ValidBookingDto();

            _patientRepository
                .Setup(x => x.GetByIdAsync(dto.PatientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Patient { PatientId = dto.PatientId });

            _doctorRepository
                .Setup(x => x.GetByIdAsync(dto.DoctorId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Doctor { DoctorId = dto.DoctorId, IsActive = true });

            _appointmentRepository
                .Setup(x => x.IsTimeSlotBookedAsync(dto.DoctorId, dto.ScheduledDate, dto.TimeSlot, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            Func<Task> act = async () => await _service.BookAppointmentAsync(dto);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("The selected time slot is already booked for this doctor.");
        }

        #endregion

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_ShouldThrowNotFoundException_WhenAppointmentDoesNotExist()
        {
            _appointmentRepository
                .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Appointment?)null);

            Func<Task> act = async () => await _service.DeleteAsync(1);

            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Appointment not found.");
        }

        [Fact]
        public async Task DeleteAsync_ShouldThrowValidationException_WhenAppointmentIsCompleted()
        {
            _appointmentRepository
                .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Appointment { AppointmentId = 1, Status = AppointmentStatus.Completed });

            Func<Task> act = async () => await _service.DeleteAsync(1);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("Completed appointments cannot be deleted.");
        }

        [Fact]
        public async Task DeleteAsync_ShouldThrowValidationException_WhenAppointmentIsConfirmed()
        {
            _appointmentRepository
                .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Appointment { AppointmentId = 1, Status = AppointmentStatus.Confirmed });

            Func<Task> act = async () => await _service.DeleteAsync(1);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("Confirmed appointments cannot be deleted.");
        }

        [Fact]
        public async Task DeleteAsync_ShouldThrowNotFoundException_WhenRepositoryDeleteReturnsNull()
        {
            _appointmentRepository
                .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Appointment { AppointmentId = 1, Status = AppointmentStatus.Pending });

            _appointmentRepository
                .Setup(x => x.DeleteAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Appointment?)null);

            Func<Task> act = async () => await _service.DeleteAsync(1);

            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Appointment not found.");
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeleteSuccessfully_WhenAppointmentIsPending()
        {
            var appointment = new Appointment { AppointmentId = 1, Status = AppointmentStatus.Pending };
            var deletedAppointment = new Appointment
            {
                AppointmentId = 1,
                PatientId = 5,
                DoctorId = 9,
                Status = AppointmentStatus.Pending
            };
            var appointmentDto = new AppointmentDto { AppointmentId = 1 };

            _appointmentRepository
                .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointment);

            _appointmentRepository
                .Setup(x => x.DeleteAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(deletedAppointment);

            _publishEndpoint
                .Setup(x => x.Publish(It.IsAny<AppointmentEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mapper
                .Setup(x => x.Map<AppointmentDto>(deletedAppointment))
                .Returns(appointmentDto);

            var result = await _service.DeleteAsync(1);

            result.Should().BeEquivalentTo(appointmentDto);

            _publishEndpoint.Verify(
                x => x.Publish(
                    It.Is<AppointmentEvent>(e => e.EventType == "AppointmentDeleted"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        #endregion

        #region CancelAppointmentByPatientAsync

        [Fact]
        public async Task CancelAppointmentByPatientAsync_ShouldThrowNotFoundException_WhenAppointmentDoesNotExist()
        {
            _appointmentRepository
                .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Appointment?)null);

            Func<Task> act = async () =>
                await _service.CancelAppointmentByPatientAsync(5, 1, new CancelAppointmentDto());

            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Appointment not found.");
        }

        [Fact]
        public async Task CancelAppointmentByPatientAsync_ShouldThrowValidationException_WhenAppointmentBelongsToDifferentPatient()
        {
            _appointmentRepository
                .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Appointment { AppointmentId = 1, PatientId = 999, Status = AppointmentStatus.Pending });

            Func<Task> act = async () =>
                await _service.CancelAppointmentByPatientAsync(5, 1, new CancelAppointmentDto());

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("This appointment does not belong to the patient.");
        }

        [Fact]
        public async Task CancelAppointmentByPatientAsync_ShouldThrowValidationException_WhenStatusIsNotPending()
        {
            _appointmentRepository
                .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Appointment { AppointmentId = 1, PatientId = 5, Status = AppointmentStatus.Confirmed });

            Func<Task> act = async () =>
                await _service.CancelAppointmentByPatientAsync(5, 1, new CancelAppointmentDto());

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("Only pending appointments can be cancelled.");
        }

        [Fact]
        public async Task CancelAppointmentByPatientAsync_ShouldThrowNotFoundException_WhenUpdateReturnsNull()
        {
            _appointmentRepository
                .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Appointment { AppointmentId = 1, PatientId = 5, Status = AppointmentStatus.Pending });

            _appointmentRepository
                .Setup(x => x.UpdateAsync(1, It.IsAny<Appointment>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Appointment?)null);

            Func<Task> act = async () =>
                await _service.CancelAppointmentByPatientAsync(5, 1, new CancelAppointmentDto());

            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Appointment not found.");
        }

        [Fact]
        public async Task CancelAppointmentByPatientAsync_ShouldCancelSuccessfully_WhenAppointmentIsPendingAndOwnedByPatient()
        {
            var appointment = new Appointment
            {
                AppointmentId = 1,
                PatientId = 5,
                DoctorId = 9,
                ScheduledDate = DateTime.Today.AddDays(1),
                Status = AppointmentStatus.Pending
            };

            var updatedAppointment = new Appointment
            {
                AppointmentId = 1,
                PatientId = 5,
                DoctorId = 9,
                ScheduledDate = appointment.ScheduledDate,
                Status = AppointmentStatus.Cancelled
            };

            var cancelDto = new CancelAppointmentDto { CancellationReason = "Change of plans" };
            var appointmentDto = new AppointmentDto { AppointmentId = 1 };

            _appointmentRepository
                .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointment);

            _appointmentRepository
                .Setup(x => x.UpdateAsync(1, It.IsAny<Appointment>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(updatedAppointment);

            _cache
                .Setup(x => x.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _publishEndpoint
                .Setup(x => x.Publish(It.IsAny<AppointmentEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mapper
                .Setup(x => x.Map<AppointmentDto>(updatedAppointment))
                .Returns(appointmentDto);

            var result = await _service.CancelAppointmentByPatientAsync(5, 1, cancelDto);

            result.Should().BeEquivalentTo(appointmentDto);

            _cache.Verify(
                x => x.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Once);

            _publishEndpoint.Verify(
                x => x.Publish(
                    It.Is<AppointmentEvent>(e => e.EventType == "PatientCancelled"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        #endregion
    }
}