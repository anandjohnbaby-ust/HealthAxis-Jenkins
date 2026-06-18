using AutoMapper;
using FluentAssertions;
using HealthAxis.API.DTOs.HealthRecordDtos;
using HealthAxis.API.Enums;
using HealthAxis.API.Exceptions;
using HealthAxis.API.Models;
using HealthAxis.API.Repositories.Interfaces;
using HealthAxis.API.Services.Implementations;
using Moq;
using Xunit;

namespace HealthAxis.Tests.Services
{
    public class HealthRecordServiceTests
    {
        private readonly Mock<IHealthRecordRepository> _healthRecordRepository;
        private readonly Mock<IAppointmentRepository> _appointmentRepository;
        private readonly Mock<IPatientRepository> _patientRepository;
        private readonly Mock<IMapper> _mapper;

        private readonly HealthRecordService _service;

        public HealthRecordServiceTests()
        {
            _healthRecordRepository =
                new Mock<IHealthRecordRepository>();

            _appointmentRepository =
                new Mock<IAppointmentRepository>();

            _patientRepository =
                new Mock<IPatientRepository>();

            _mapper =
                new Mock<IMapper>();

            _service =
                new HealthRecordService(
                    _healthRecordRepository.Object,
                    _appointmentRepository.Object,
                    _mapper.Object);
        }

        private Appointment GetAppointment(
            AppointmentStatus status = AppointmentStatus.Completed)
        {
            return new Appointment
            {
                AppointmentId = 1,
                DoctorId = 1,
                PatientId = 1,
                ScheduledDate = DateTime.Today,
                TimeSlot = "10:00 AM",
                Status = status
            };
        }

        private HealthRecord GetHealthRecord()
        {
            return new HealthRecord
            {
                RecordId = 1,
                AppointmentId = 1,
                DoctorId = 1,
                PatientId = 1,
                VisitDate = DateTime.Today,
                Diagnosis = "Fever",
                Prescription = "Paracetamol",
                Notes = "Take rest"
            };
        }

        private HealthRecordDto GetHealthRecordDto()
        {
            return new HealthRecordDto
            {
                RecordId = 1,
                AppointmentId = 1,
                DoctorId = 1,
                PatientId = 1,
                VisitDate = DateTime.Today,
                Diagnosis = "Fever",
                Prescription = "Paracetamol",
                Notes = "Take rest"
            };
        }

        private CreateHealthRecordDto GetCreateHealthRecordDto()
        {
            return new CreateHealthRecordDto
            {
                AppointmentId = 1,
                Diagnosis = "Fever",
                Prescription = "Paracetamol",
                Notes = "Take Rest"
            };
        }

        [Fact]
        public async Task GetByRecordIdAsync_ShouldReturnHealthRecord()
        {
            // Arrange

            var record = GetHealthRecord();

            var dto = GetHealthRecordDto();

            _healthRecordRepository
                .Setup(x => x.GetByIdAsync(
                    1,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(record);

            _mapper
                .Setup(x => x.Map<HealthRecordDto>(record))
                .Returns(dto);

            // Act

            var result =
                await _service.GetByRecordIdAsync(1);

            // Assert

            result.Should().NotBeNull();

            result.RecordId.Should().Be(1);

            result.Diagnosis.Should().Be("Fever");
        }

        [Fact]
        public async Task GetByRecordIdAsync_ShouldThrowNotFoundException_WhenRecordDoesNotExist()
        {
            // Arrange

            _healthRecordRepository
                .Setup(x => x.GetByIdAsync(
                    100,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((HealthRecord?)null);

            // Act

            Func<Task> action =
                async () => await _service.GetByRecordIdAsync(100);

            // Assert

            await action.Should()
                .ThrowAsync<NotFoundException>()
                .WithMessage("Health record not found.");
        }

        [Fact]
        public async Task GetByRecordIdAsync_ShouldThrowException_WhenRepositoryFails()
        {
            // Arrange

            _healthRecordRepository
                .Setup(x => x.GetByIdAsync(
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database Error"));

            // Act

            Func<Task> action =
                async () => await _service.GetByRecordIdAsync(1);

            // Assert

            await action.Should()
                .ThrowAsync<Exception>()
                .WithMessage("Database Error");
        }

        [Fact]
        public async Task GetByRecordIdAsync_ShouldCallRepositoryOnce()
        {
            var record = GetHealthRecord();

            var dto = GetHealthRecordDto();

            _healthRecordRepository
                .Setup(x => x.GetByIdAsync(
                    1,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(record);

            _mapper
                .Setup(x => x.Map<HealthRecordDto>(record))
                .Returns(dto);

            await _service.GetByRecordIdAsync(1);

            _healthRecordRepository.Verify(
                x => x.GetByIdAsync(
                    1,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetByRecordIdAsync_ShouldCallMapperOnce()
        {
            var record = GetHealthRecord();

            var dto = GetHealthRecordDto();

            _healthRecordRepository
                .Setup(x => x.GetByIdAsync(
                    1,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(record);

            _mapper
                .Setup(x => x.Map<HealthRecordDto>(record))
                .Returns(dto);

            await _service.GetByRecordIdAsync(1);

            _mapper.Verify(
                x => x.Map<HealthRecordDto>(record),
                Times.Once);
        }

        [Fact]
        public async Task GetByRecordIdAsync_ShouldPassCancellationToken()
        {
            var token = new CancellationToken();

            var record = GetHealthRecord();

            var dto = GetHealthRecordDto();

            _healthRecordRepository
                .Setup(x => x.GetByIdAsync(1, token))
                .ReturnsAsync(record);

            _mapper
                .Setup(x => x.Map<HealthRecordDto>(record))
                .Returns(dto);

            await _service.GetByRecordIdAsync(1, token);

            _healthRecordRepository.Verify(
                x => x.GetByIdAsync(1, token),
                Times.Once);
        }

        [Fact]
        public async Task AddAsync_ShouldCreateHealthRecord()
        {
            // Arrange

            var dto = GetCreateHealthRecordDto();

            var appointment = GetAppointment(AppointmentStatus.Completed);

            var savedRecord = GetHealthRecord();

            var response = GetHealthRecordDto();

            _appointmentRepository
                .Setup(x => x.GetByIdAsync(dto.AppointmentId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointment);

            _healthRecordRepository
                .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<HealthRecord>());

            _healthRecordRepository
                .Setup(x => x.AddAsync(It.IsAny<HealthRecord>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(savedRecord);

            _mapper
                .Setup(x => x.Map<HealthRecordDto>(savedRecord))
                .Returns(response);

            // Act

            var result = await _service.AddAsync(dto);

            // Assert

            result.Should().NotBeNull();

            result.RecordId.Should().Be(1);

            result.Diagnosis.Should().Be("Fever");
        }

        [Fact]
        public async Task AddAsync_ShouldThrowNotFoundException_WhenAppointmentDoesNotExist()
        {
            var dto = GetCreateHealthRecordDto();

            _appointmentRepository
                .Setup(x => x.GetByIdAsync(dto.AppointmentId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Appointment?)null);

            Func<Task> action =
                async () => await _service.AddAsync(dto);

            await action.Should()
                .ThrowAsync<NotFoundException>()
                .WithMessage("Appointment not found.");
        }

        [Fact]
        public async Task AddAsync_ShouldThrowValidationException_WhenAppointmentNotCompleted()
        {
            var dto = GetCreateHealthRecordDto();

            var appointment =
                GetAppointment(AppointmentStatus.Pending);

            _appointmentRepository
                .Setup(x => x.GetByIdAsync(dto.AppointmentId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointment);

            Func<Task> action =
                async () => await _service.AddAsync(dto);

            await action.Should()
                .ThrowAsync<ValidationException>()
                .WithMessage("Health records can only be created for completed appointments.");
        }

        [Fact]
        public async Task AddAsync_ShouldThrowValidationException_WhenHealthRecordAlreadyExists()
        {
            var dto = GetCreateHealthRecordDto();

            var appointment =
                GetAppointment(AppointmentStatus.Completed);

            var existingRecords = new List<HealthRecord>
    {
        new HealthRecord
        {
            AppointmentId = dto.AppointmentId
        }
    };

            _appointmentRepository
                .Setup(x => x.GetByIdAsync(dto.AppointmentId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointment);

            _healthRecordRepository
                .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingRecords);

            Func<Task> action =
                async () => await _service.AddAsync(dto);

            await action.Should()
                .ThrowAsync<ValidationException>()
                .WithMessage("Health record already exists for this appointment.");
        }

        [Fact]
        public async Task AddAsync_ShouldCallRepositoryAddOnce()
        {
            var dto = GetCreateHealthRecordDto();

            var appointment =
                GetAppointment(AppointmentStatus.Completed);

            var record = GetHealthRecord();

            _appointmentRepository
                .Setup(x => x.GetByIdAsync(dto.AppointmentId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointment);

            _healthRecordRepository
                .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<HealthRecord>());

            _healthRecordRepository
                .Setup(x => x.AddAsync(It.IsAny<HealthRecord>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(record);

            _mapper
                .Setup(x => x.Map<HealthRecordDto>(record))
                .Returns(GetHealthRecordDto());

            await _service.AddAsync(dto);

            _healthRecordRepository.Verify(
                x => x.AddAsync(It.IsAny<HealthRecord>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task AddAsync_ShouldCallMapperOnce()
        {
            var dto = GetCreateHealthRecordDto();

            var appointment =
                GetAppointment(AppointmentStatus.Completed);

            var record = GetHealthRecord();

            _appointmentRepository
                .Setup(x => x.GetByIdAsync(dto.AppointmentId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointment);

            _healthRecordRepository
                .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<HealthRecord>());

            _healthRecordRepository
                .Setup(x => x.AddAsync(It.IsAny<HealthRecord>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(record);

            _mapper
                .Setup(x => x.Map<HealthRecordDto>(record))
                .Returns(GetHealthRecordDto());

            await _service.AddAsync(dto);

            _mapper.Verify(
                x => x.Map<HealthRecordDto>(record),
                Times.Once);
        }

        [Fact]
        public async Task AddAsync_ShouldCopyAppointmentDetailsToHealthRecord()
        {
            var dto = GetCreateHealthRecordDto();

            var appointment =
                GetAppointment(AppointmentStatus.Completed);

            HealthRecord? capturedRecord = null;

            _appointmentRepository
                .Setup(x => x.GetByIdAsync(dto.AppointmentId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointment);

            _healthRecordRepository
                .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<HealthRecord>());

            _healthRecordRepository
                .Setup(x => x.AddAsync(It.IsAny<HealthRecord>(),
                    It.IsAny<CancellationToken>()))
                .Callback<HealthRecord, CancellationToken>((record, _) =>
                {
                    capturedRecord = record;
                })
                .ReturnsAsync(GetHealthRecord());

            _mapper
                .Setup(x => x.Map<HealthRecordDto>(It.IsAny<HealthRecord>()))
                .Returns(GetHealthRecordDto());

            await _service.AddAsync(dto);

            capturedRecord.Should().NotBeNull();

            capturedRecord!.DoctorId.Should().Be(appointment.DoctorId);

            capturedRecord.PatientId.Should().Be(appointment.PatientId);

            capturedRecord.VisitDate.Should().Be(appointment.ScheduledDate);
        }

        [Fact]
        public async Task AddAsync_ShouldPassCancellationToken()
        {
            var token = new CancellationToken();

            var dto = GetCreateHealthRecordDto();

            var appointment =
                GetAppointment(AppointmentStatus.Completed);

            var record = GetHealthRecord();

            _appointmentRepository
                .Setup(x => x.GetByIdAsync(dto.AppointmentId, token))
                .ReturnsAsync(appointment);

            _healthRecordRepository
                .Setup(x => x.GetAllAsync(token))
                .ReturnsAsync(new List<HealthRecord>());

            _healthRecordRepository
                .Setup(x => x.AddAsync(It.IsAny<HealthRecord>(), token))
                .ReturnsAsync(record);

            _mapper
                .Setup(x => x.Map<HealthRecordDto>(record))
                .Returns(GetHealthRecordDto());

            await _service.AddAsync(dto, token);

            _appointmentRepository.Verify(
                x => x.GetByIdAsync(dto.AppointmentId, token),
                Times.Once);

            _healthRecordRepository.Verify(
                x => x.GetAllAsync(token),
                Times.Once);

            _healthRecordRepository.Verify(
                x => x.AddAsync(It.IsAny<HealthRecord>(), token),
                Times.Once);
        }

        [Fact]
        public async Task GetByPatientIdAsync_ShouldReturnHealthRecords()
        {
            // Arrange

            var records = new List<HealthRecord>
    {
        GetHealthRecord()
    };

            var dtos = new List<HealthRecordDto>
    {
        GetHealthRecordDto()
    };

            _healthRecordRepository
                .Setup(x => x.GetByPatientIdAsync(
                    1,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(records);

            _mapper
                .Setup(x => x.Map<IEnumerable<HealthRecordDto>>(records))
                .Returns(dtos);

            // Act

            var result =
                await _service.GetByPatientIdAsync(1);

            // Assert

            result.Should().NotBeNull();

            result.Should().HaveCount(1);

            result.First().Diagnosis.Should().Be("Fever");
        }

        [Fact]
        public async Task GetByPatientIdAsync_ShouldThrowNotFoundException_WhenNoRecordsExist()
        {
            // Arrange

            _healthRecordRepository
                .Setup(x => x.GetByPatientIdAsync(
                    1,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<HealthRecord>());

            // Act

            Func<Task> action =
                async () => await _service.GetByPatientIdAsync(1);

            // Assert

            await action.Should()
                .ThrowAsync<NotFoundException>()
                .WithMessage("No health records found for patient with ID 1.");
        }

        [Fact]
        public async Task GetByPatientIdAsync_ShouldThrowException_WhenRepositoryFails()
        {
            // Arrange

            _healthRecordRepository
                .Setup(x => x.GetByPatientIdAsync(
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database Error"));

            // Act

            Func<Task> action =
                async () => await _service.GetByPatientIdAsync(1);

            // Assert

            await action.Should()
                .ThrowAsync<Exception>()
                .WithMessage("Database Error");
        }

        [Fact]
        public async Task GetByPatientIdAsync_ShouldCallRepositoryOnce()
        {
            // Arrange

            var records = new List<HealthRecord>
    {
        GetHealthRecord()
    };

            var dtos = new List<HealthRecordDto>
    {
        GetHealthRecordDto()
    };

            _healthRecordRepository
                .Setup(x => x.GetByPatientIdAsync(
                    1,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(records);

            _mapper
                .Setup(x => x.Map<IEnumerable<HealthRecordDto>>(records))
                .Returns(dtos);

            // Act

            await _service.GetByPatientIdAsync(1);

            // Assert

            _healthRecordRepository.Verify(
                x => x.GetByPatientIdAsync(
                    1,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetByPatientIdAsync_ShouldCallMapperOnce()
        {
            // Arrange

            var records = new List<HealthRecord>
    {
        GetHealthRecord()
    };

            var dtos = new List<HealthRecordDto>
    {
        GetHealthRecordDto()
    };

            _healthRecordRepository
                .Setup(x => x.GetByPatientIdAsync(
                    1,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(records);

            _mapper
                .Setup(x => x.Map<IEnumerable<HealthRecordDto>>(records))
                .Returns(dtos);

            // Act

            await _service.GetByPatientIdAsync(1);

            // Assert

            _mapper.Verify(
                x => x.Map<IEnumerable<HealthRecordDto>>(records),
                Times.Once);
        }

        [Fact]
        public async Task GetByPatientIdAsync_ShouldPassCancellationToken()
        {
            // Arrange

            var token = new CancellationToken();

            var records = new List<HealthRecord>
    {
        GetHealthRecord()
    };

            var dtos = new List<HealthRecordDto>
    {
        GetHealthRecordDto()
    };

            _healthRecordRepository
                .Setup(x => x.GetByPatientIdAsync(1, token))
                .ReturnsAsync(records);

            _mapper
                .Setup(x => x.Map<IEnumerable<HealthRecordDto>>(records))
                .Returns(dtos);

            // Act

            await _service.GetByPatientIdAsync(1, token);

            // Assert

            _healthRecordRepository.Verify(
                x => x.GetByPatientIdAsync(1, token),
                Times.Once);
        }
    }
}