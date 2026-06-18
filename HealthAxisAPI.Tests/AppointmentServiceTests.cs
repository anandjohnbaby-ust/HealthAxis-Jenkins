using AutoMapper;
using FluentAssertions;
using HealthAxis.API.DTOs.AppointmentDtos;
using HealthAxis.API.Enums;
using HealthAxis.API.Exceptions;
using HealthAxis.API.Models;
using HealthAxis.API.Repositories.Interfaces;
using HealthAxis.API.Services.Implementations;
using Moq;
using Xunit;

namespace HealthAxis.Tests.Services
{
    public class AppointmentServiceTests
    {
        private readonly Mock<IRepository<Appointment>> _appointmentRepository;
        private readonly Mock<IRepository<Doctor>> _doctorRepository;
        private readonly Mock<IPatientRepository> _patientRepository;
        private readonly Mock<IMapper> _mapper;

        private readonly AppointmentService _service;

        public AppointmentServiceTests()
        {
            _appointmentRepository =
                new Mock<IRepository<Appointment>>();

            _doctorRepository =
                new Mock<IRepository<Doctor>>();

            _patientRepository =
                new Mock<IPatientRepository>();

            _mapper =
                new Mock<IMapper>();

            _service =
                new AppointmentService(
                    _appointmentRepository.Object,
                    _doctorRepository.Object,
                    _patientRepository.Object,
                    _mapper.Object);
        }

        private List<Appointment> GetAppointments()
        {
            return new List<Appointment>
            {
                new Appointment
                {
                    AppointmentId = 1,
                    PatientId = 1,
                    DoctorId = 1,
                    ScheduledDate = DateTime.Today.AddDays(2),
                    TimeSlot = "10:00 AM",
                    Status = AppointmentStatus.Pending
                },

                new Appointment
                {
                    AppointmentId = 2,
                    PatientId = 2,
                    DoctorId = 2,
                    ScheduledDate = DateTime.Today.AddDays(3),
                    TimeSlot = "11:00 AM",
                    Status = AppointmentStatus.Confirmed
                }
            };
        }

        private List<AppointmentDto> GetAppointmentDtos()
        {
            return new List<AppointmentDto>
            {
                new AppointmentDto
                {
                    AppointmentId = 1,
                    PatientId = 1,
                    DoctorId = 1,
                    ScheduledDate = DateTime.Today.AddDays(2),
                    TimeSlot = "10:00 AM",
                    Status = AppointmentStatus.Pending
                },

                new AppointmentDto
                {
                    AppointmentId = 2,
                    PatientId = 2,
                    DoctorId = 2,
                    ScheduledDate = DateTime.Today.AddDays(3),
                    TimeSlot = "11:00 AM",
                    Status = AppointmentStatus.Confirmed
                }
            };
        }

        private Patient GetPatient()
        {
            return new Patient
            {
                PatientId = 1,
                FullName = "John",
                Email = "john@test.com",
                PhoneNumber = "9876543210"
            };
        }

        private Doctor GetDoctor(bool isActive = true)
        {
            return new Doctor
            {
                DoctorId = 1,
                FullName = "Dr John",
                IsActive = isActive,
                ConsultationFee = 500
            };
        }

        private CreateAppointmentDto GetCreateDto()
        {
            return new CreateAppointmentDto
            {
                PatientId = 1,
                DoctorId = 1,
                ScheduledDate = DateTime.Today.AddDays(2),
                TimeSlot = "10:00 AM"
            };
        }

        private Appointment GetAppointment(AppointmentStatus status = AppointmentStatus.Pending)
        {
            return new Appointment
            {
                AppointmentId = 1,
                PatientId = 1,
                DoctorId = 1,
                ScheduledDate = DateTime.Today.AddDays(2),
                TimeSlot = "10:00 AM",
                Status = status
            };
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllAppointments()
        {
            // Arrange

            var appointments = GetAppointments();

            var dtos = GetAppointmentDtos();

            _appointmentRepository
                .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointments);

            _mapper
                .Setup(x => x.Map<IEnumerable<AppointmentDto>>(appointments))
                .Returns(dtos);

            // Act

            var result = await _service.GetAllAsync();

            // Assert

            result.Should().NotBeNull();

            result.Should().HaveCount(2);

            result.First().AppointmentId.Should().Be(1);

            _appointmentRepository.Verify(
                x => x.GetAllAsync(It.IsAny<CancellationToken>()),
                Times.Once);

            _mapper.Verify(
                x => x.Map<IEnumerable<AppointmentDto>>(appointments),
                Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnEmptyCollection_WhenRepositoryReturnsEmpty()
        {
            // Arrange

            var appointments = new List<Appointment>();

            var dtos = new List<AppointmentDto>();

            _appointmentRepository
                .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointments);

            _mapper
                .Setup(x => x.Map<IEnumerable<AppointmentDto>>(appointments))
                .Returns(dtos);

            // Act

            var result = await _service.GetAllAsync();

            // Assert

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllAsync_ShouldThrowException_WhenRepositoryFails()
        {
            // Arrange

            _appointmentRepository
                .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database Error"));

            // Act

            Func<Task> action =
                async () => await _service.GetAllAsync();

            // Assert

            await action.Should()
                .ThrowAsync<Exception>()
                .WithMessage("Database Error");
        }

        [Fact]
        public async Task GetAllAsync_ShouldCallMapperOnce()
        {
            // Arrange

            var appointments = GetAppointments();

            var dtos = GetAppointmentDtos();

            _appointmentRepository
                .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointments);

            _mapper
                .Setup(x => x.Map<IEnumerable<AppointmentDto>>(appointments))
                .Returns(dtos);

            // Act

            await _service.GetAllAsync();

            // Assert

            _mapper.Verify(
                x => x.Map<IEnumerable<AppointmentDto>>(appointments),
                Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_ShouldPassCancellationToken()
        {
            // Arrange

            var token = new CancellationToken();

            var appointments = GetAppointments();

            var dtos = GetAppointmentDtos();

            _appointmentRepository
                .Setup(x => x.GetAllAsync(token))
                .ReturnsAsync(appointments);

            _mapper
                .Setup(x => x.Map<IEnumerable<AppointmentDto>>(appointments))
                .Returns(dtos);

            // Act

            await _service.GetAllAsync(token);

            // Assert

            _appointmentRepository.Verify(
                x => x.GetAllAsync(token),
                Times.Once);
        }

        [Fact]
        public async Task AddAsync_ShouldCreateAppointment()
        {
            // Arrange

            var dto = GetCreateDto();

            var patient = GetPatient();

            var doctor = GetDoctor();

            var appointment = new Appointment();

            var appointmentDto = new AppointmentDto
            {
                AppointmentId = 1,
                PatientId = 1,
                DoctorId = 1,
                Status = AppointmentStatus.Pending
            };

            _patientRepository
                .Setup(x => x.GetByIdAsync(dto.PatientId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(patient);

            _doctorRepository
                .Setup(x => x.GetByIdAsync(dto.DoctorId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(doctor);

            _mapper
                .Setup(x => x.Map<Appointment>(dto))
                .Returns(appointment);

            _appointmentRepository
                .Setup(x => x.AddAsync(appointment,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointment);

            _mapper
                .Setup(x => x.Map<AppointmentDto>(appointment))
                .Returns(appointmentDto);

            // Act

            var result = await _service.AddAsync(dto);

            // Assert

            result.Should().NotBeNull();

            result.Status.Should().Be(AppointmentStatus.Pending);
        }

        [Fact]
        public async Task AddAsync_ShouldThrowValidationException_WhenDateIsPast()
        {
            // Arrange

            var dto = GetCreateDto();

            dto.ScheduledDate = DateTime.Today.AddDays(-1);

            // Act

            Func<Task> action = async () =>
                await _service.AddAsync(dto);

            // Assert

            await action.Should()
                .ThrowAsync<ValidationException>()
                .WithMessage("Appointments cannot be booked for past dates.");
        }

        [Fact]
        public async Task AddAsync_ShouldThrowValidationException_WhenDateExceedsSixMonths()
        {
            var dto = GetCreateDto();

            dto.ScheduledDate = DateTime.Today.AddMonths(7);

            Func<Task> action = async () =>
                await _service.AddAsync(dto);

            await action.Should()
                .ThrowAsync<ValidationException>()
                .WithMessage("Appointments can only be booked up to 6 months in advance.");
        }

        [Fact]
        public async Task AddAsync_ShouldThrowNotFound_WhenPatientDoesNotExist()
        {
            var dto = GetCreateDto();

            _patientRepository
                .Setup(x => x.GetByIdAsync(dto.PatientId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Patient?)null);

            Func<Task> action = async () =>
                await _service.AddAsync(dto);

            await action.Should()
                .ThrowAsync<NotFoundException>()
                .WithMessage("Patient not found.");
        }

        [Fact]
        public async Task AddAsync_ShouldThrowNotFound_WhenDoctorDoesNotExist()
        {
            var dto = GetCreateDto();

            _patientRepository
                .Setup(x => x.GetByIdAsync(dto.PatientId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(GetPatient());

            _doctorRepository
                .Setup(x => x.GetByIdAsync(dto.DoctorId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Doctor?)null);

            Func<Task> action = async () =>
                await _service.AddAsync(dto);

            await action.Should()
                .ThrowAsync<NotFoundException>()
                .WithMessage("Doctor not found.");
        }

        [Fact]
        public async Task AddAsync_ShouldThrowValidationException_WhenDoctorInactive()
        {
            var dto = GetCreateDto();

            _patientRepository
                .Setup(x => x.GetByIdAsync(dto.PatientId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(GetPatient());

            _doctorRepository
                .Setup(x => x.GetByIdAsync(dto.DoctorId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(GetDoctor(false));

            Func<Task> action = async () =>
                await _service.AddAsync(dto);

            await action.Should()
                .ThrowAsync<ValidationException>()
                .WithMessage("Appointments cannot be booked with inactive doctors.");
        }

        [Fact]
        public async Task AddAsync_ShouldSetStatusToPending()
        {
            var dto = GetCreateDto();

            var appointment = new Appointment();

            _patientRepository
                .Setup(x => x.GetByIdAsync(dto.PatientId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(GetPatient());

            _doctorRepository
                .Setup(x => x.GetByIdAsync(dto.DoctorId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(GetDoctor());

            _mapper
                .Setup(x => x.Map<Appointment>(dto))
                .Returns(appointment);

            _appointmentRepository
                .Setup(x => x.AddAsync(appointment,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointment);

            _mapper
                .Setup(x => x.Map<AppointmentDto>(appointment))
                .Returns(new AppointmentDto());

            await _service.AddAsync(dto);

            appointment.Status.Should().Be(AppointmentStatus.Pending);
        }

        [Fact]
        public async Task AddAsync_ShouldCallRepositoryOnce()
        {
            var dto = GetCreateDto();

            var appointment = new Appointment();

            _patientRepository
                .Setup(x => x.GetByIdAsync(dto.PatientId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(GetPatient());

            _doctorRepository
                .Setup(x => x.GetByIdAsync(dto.DoctorId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(GetDoctor());

            _mapper
                .Setup(x => x.Map<Appointment>(dto))
                .Returns(appointment);

            _appointmentRepository
                .Setup(x => x.AddAsync(appointment,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointment);

            _mapper
                .Setup(x => x.Map<AppointmentDto>(appointment))
                .Returns(new AppointmentDto());

            await _service.AddAsync(dto);

            _appointmentRepository.Verify(
                x => x.AddAsync(appointment,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateStatusAsync_ShouldConfirmAppointment()
        {
            var appointment = GetAppointment(AppointmentStatus.Pending);

            var dto = new UpdateAppointmentStatusDto
            {
                Status = AppointmentStatus.Confirmed
            };

            _appointmentRepository
                .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointment);

            _appointmentRepository
                .Setup(x => x.UpdateAsync(1, appointment, It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointment);

            _mapper
                .Setup(x => x.Map<AppointmentDto>(appointment))
                .Returns(new AppointmentDto
                {
                    Status = AppointmentStatus.Confirmed
                });

            var result = await _service.UpdateStatusAsync(1, dto);

            result.Status.Should().Be(AppointmentStatus.Confirmed);
        }

        [Fact]
        public async Task UpdateStatusAsync_ShouldCancelAppointment()
        {
            var appointment = GetAppointment(AppointmentStatus.Confirmed);

            var dto = new UpdateAppointmentStatusDto
            {
                Status = AppointmentStatus.Cancelled,
                CancellationReason = "Patient Sick"
            };

            _appointmentRepository
                .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointment);

            _appointmentRepository
                .Setup(x => x.UpdateAsync(1, appointment, It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointment);

            _mapper
                .Setup(x => x.Map<AppointmentDto>(appointment))
                .Returns(new AppointmentDto
                {
                    Status = AppointmentStatus.Cancelled,
                    CancellationReason = "Patient Sick"
                });

            var result = await _service.UpdateStatusAsync(1, dto);

            result.Status.Should().Be(AppointmentStatus.Cancelled);

            result.CancellationReason.Should().Be("Patient Sick");
        }

        [Fact]
        public async Task UpdateStatusAsync_ShouldCompleteAppointment()
        {
            var appointment = GetAppointment(AppointmentStatus.Confirmed);

            var dto = new UpdateAppointmentStatusDto
            {
                Status = AppointmentStatus.Completed
            };

            _appointmentRepository
                .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointment);

            _appointmentRepository
                .Setup(x => x.UpdateAsync(1, appointment, It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointment);

            _mapper
                .Setup(x => x.Map<AppointmentDto>(appointment))
                .Returns(new AppointmentDto
                {
                    Status = AppointmentStatus.Completed
                });

            var result = await _service.UpdateStatusAsync(1, dto);

            result.Status.Should().Be(AppointmentStatus.Completed);
        }

        [Fact]
        public async Task UpdateStatusAsync_ShouldThrowNotFound_WhenAppointmentDoesNotExist()
        {
            _appointmentRepository
                .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Appointment?)null);

            var dto = new UpdateAppointmentStatusDto
            {
                Status = AppointmentStatus.Confirmed
            };

            Func<Task> action =
                async () => await _service.UpdateStatusAsync(1, dto);

            await action.Should()
                .ThrowAsync<NotFoundException>()
                .WithMessage("Appointment not found.");
        }

        [Fact]
        public async Task UpdateStatusAsync_ShouldThrow_WhenAlreadyCancelled()
        {
            var appointment =
                GetAppointment(AppointmentStatus.Cancelled);

            _appointmentRepository
                .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointment);

            var dto = new UpdateAppointmentStatusDto
            {
                Status = AppointmentStatus.Completed
            };

            Func<Task> action =
                async () => await _service.UpdateStatusAsync(1, dto);

            await action.Should()
                .ThrowAsync<ValidationException>()
                .WithMessage("Cancelled appointments cannot be modified.");
        }

        [Fact]
        public async Task UpdateStatusAsync_ShouldThrow_WhenAlreadyCompleted()
        {
            var appointment =
                GetAppointment(AppointmentStatus.Completed);

            _appointmentRepository
                .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointment);

            var dto = new UpdateAppointmentStatusDto
            {
                Status = AppointmentStatus.Cancelled
            };

            Func<Task> action =
                async () => await _service.UpdateStatusAsync(1, dto);

            await action.Should()
                .ThrowAsync<ValidationException>()
                .WithMessage("Completed appointments cannot be modified.");
        }

        [Fact]
        public async Task UpdateStatusAsync_ShouldThrow_WhenStatusAlreadyExists()
        {
            var appointment =
                GetAppointment(AppointmentStatus.Pending);

            _appointmentRepository
                .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointment);

            var dto = new UpdateAppointmentStatusDto
            {
                Status = AppointmentStatus.Pending
            };

            Func<Task> action =
                async () => await _service.UpdateStatusAsync(1, dto);

            await action.Should()
                .ThrowAsync<ValidationException>()
                .WithMessage("Appointment is already Pending.");
        }

        [Fact]
        public async Task UpdateStatusAsync_ShouldThrow_WhenPendingDirectlyCompleted()
        {
            var appointment =
                GetAppointment(AppointmentStatus.Pending);

            _appointmentRepository
                .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointment);

            var dto = new UpdateAppointmentStatusDto
            {
                Status = AppointmentStatus.Completed
            };

            Func<Task> action =
                async () => await _service.UpdateStatusAsync(1, dto);

            await action.Should()
                .ThrowAsync<ValidationException>()
                .WithMessage("Pending appointments must be confirmed before completion.");
        }

        [Fact]
        public async Task UpdateStatusAsync_ShouldThrow_WhenRevertingToPending()
        {
            var appointment =
                GetAppointment(AppointmentStatus.Confirmed);

            _appointmentRepository
                .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointment);

            var dto = new UpdateAppointmentStatusDto
            {
                Status = AppointmentStatus.Pending
            };

            Func<Task> action =
                async () => await _service.UpdateStatusAsync(1, dto);

            await action.Should()
                .ThrowAsync<ValidationException>()
                .WithMessage("Appointments cannot be reverted to pending status.");
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeletePendingAppointment()
        {
            // Arrange

            var appointment = GetAppointment(AppointmentStatus.Pending);

            var dto = new AppointmentDto
            {
                AppointmentId = appointment.AppointmentId,
                Status = AppointmentStatus.Pending
            };

            _appointmentRepository
                .Setup(x => x.GetByIdAsync(1,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointment);

            _appointmentRepository
                .Setup(x => x.DeleteAsync(1,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointment);

            _mapper
                .Setup(x => x.Map<AppointmentDto>(appointment))
                .Returns(dto);

            // Act

            var result = await _service.DeleteAsync(1);

            // Assert

            result.Should().NotBeNull();

            result.AppointmentId.Should().Be(1);
        }

        [Fact]
        public async Task DeleteAsync_ShouldThrowNotFoundException_WhenAppointmentDoesNotExist()
        {
            // Arrange

            _appointmentRepository
                .Setup(x => x.GetByIdAsync(100,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Appointment?)null);

            // Act

            Func<Task> action =
                async () => await _service.DeleteAsync(100);

            // Assert

            await action.Should()
                .ThrowAsync<NotFoundException>()
                .WithMessage("Appointment not found.");
        }

        [Fact]
        public async Task DeleteAsync_ShouldThrowValidationException_WhenAppointmentCompleted()
        {
            // Arrange

            var appointment =
                GetAppointment(AppointmentStatus.Completed);

            _appointmentRepository
                .Setup(x => x.GetByIdAsync(1,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointment);

            // Act

            Func<Task> action =
                async () => await _service.DeleteAsync(1);

            // Assert

            await action.Should()
                .ThrowAsync<ValidationException>()
                .WithMessage("Completed appointments cannot be deleted.");
        }

        [Fact]
        public async Task DeleteAsync_ShouldThrowValidationException_WhenAppointmentConfirmed()
        {
            // Arrange

            var appointment =
                GetAppointment(AppointmentStatus.Confirmed);

            _appointmentRepository
                .Setup(x => x.GetByIdAsync(1,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointment);

            // Act

            Func<Task> action =
                async () => await _service.DeleteAsync(1);

            // Assert

            await action.Should()
                .ThrowAsync<ValidationException>()
                .WithMessage("Confirmed appointments cannot be deleted.");
        }

        [Fact]
        public async Task DeleteAsync_ShouldCallDeleteRepositoryOnce()
        {
            // Arrange

            var appointment =
                GetAppointment(AppointmentStatus.Pending);

            _appointmentRepository
                .Setup(x => x.GetByIdAsync(1,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointment);

            _appointmentRepository
                .Setup(x => x.DeleteAsync(1,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointment);

            _mapper
                .Setup(x => x.Map<AppointmentDto>(appointment))
                .Returns(new AppointmentDto());

            // Act

            await _service.DeleteAsync(1);

            // Assert

            _appointmentRepository.Verify(
                x => x.DeleteAsync(
                    1,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldCallMapperOnce()
        {
            // Arrange

            var appointment =
                GetAppointment(AppointmentStatus.Pending);

            _appointmentRepository
                .Setup(x => x.GetByIdAsync(1,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointment);

            _appointmentRepository
                .Setup(x => x.DeleteAsync(1,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointment);

            _mapper
                .Setup(x => x.Map<AppointmentDto>(appointment))
                .Returns(new AppointmentDto());

            // Act

            await _service.DeleteAsync(1);

            // Assert

            _mapper.Verify(
                x => x.Map<AppointmentDto>(appointment),
                Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldPassCancellationToken()
        {
            // Arrange

            var token = new CancellationToken();

            var appointment =
                GetAppointment(AppointmentStatus.Pending);

            _appointmentRepository
                .Setup(x => x.GetByIdAsync(1, token))
                .ReturnsAsync(appointment);

            _appointmentRepository
                .Setup(x => x.DeleteAsync(1, token))
                .ReturnsAsync(appointment);

            _mapper
                .Setup(x => x.Map<AppointmentDto>(appointment))
                .Returns(new AppointmentDto());

            // Act

            await _service.DeleteAsync(1, token);

            // Assert

            _appointmentRepository.Verify(
                x => x.GetByIdAsync(1, token),
                Times.Once);

            _appointmentRepository.Verify(
                x => x.DeleteAsync(1, token),
                Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldThrowException_WhenRepositoryFails()
        {
            // Arrange

            var appointment =
                GetAppointment(AppointmentStatus.Pending);

            _appointmentRepository
                .Setup(x => x.GetByIdAsync(1,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointment);

            _appointmentRepository
                .Setup(x => x.DeleteAsync(1,
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database Error"));

            // Act

            Func<Task> action =
                async () => await _service.DeleteAsync(1);

            // Assert

            await action.Should()
                .ThrowAsync<Exception>()
                .WithMessage("Database Error");
        }

    }
}