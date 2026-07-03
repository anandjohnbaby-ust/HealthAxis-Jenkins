using AutoMapper;
using FluentAssertions;
using HealthAxis.Shared.DTOs.HealthRecordDtos;
using HealthAxis.Shared.Enums;
using HealthAxis.API.Exceptions;
using HealthAxis.API.Models;
using HealthAxis.API.Repositories.Interfaces;
using HealthAxis.API.Services.Implementations;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
            _healthRecordRepository = new Mock<IHealthRecordRepository>();
            _appointmentRepository = new Mock<IAppointmentRepository>();
            _patientRepository = new Mock<IPatientRepository>();
            _mapper = new Mock<IMapper>();

            _service = new HealthRecordService(
                _healthRecordRepository.Object,
                _appointmentRepository.Object,
                _mapper.Object);
        }

        private Appointment GetAppointment(AppointmentStatus status = AppointmentStatus.Completed)
        {
            return new Appointment
            {
                AppointmentId = 1,
                DoctorId = 1,
                PatientId = 1,
                ScheduledDate = DateTime.Today,
                TimeSlot = new TimeOnly(10, 0),
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
            var record = GetHealthRecord();
            var dto = GetHealthRecordDto();

            _healthRecordRepository.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(record);

            _mapper.Setup(x => x.Map<HealthRecordDto>(record)).Returns(dto);

            var result = await _service.GetByRecordIdAsync(1);

            result.Should().NotBeNull();
            result.RecordId.Should().Be(1);
            result.Diagnosis.Should().Be("Fever");
        }

        [Fact]
        public async Task GetByRecordIdAsync_ShouldThrowNotFoundException_WhenRecordDoesNotExist()
        {
            _healthRecordRepository.Setup(x => x.GetByIdAsync(100, It.IsAny<CancellationToken>())).ReturnsAsync((HealthRecord?)null);

            Func<Task> action = async () => await _service.GetByRecordIdAsync(100);

            await action.Should().ThrowAsync<NotFoundException>().WithMessage("Health record not found.");
        }

        [Fact]
        public async Task GetByRecordIdAsync_ShouldThrowException_WhenRepositoryFails()
        {
            _healthRecordRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Database Error"));

            Func<Task> action = async () => await _service.GetByRecordIdAsync(1);

            await action.Should().ThrowAsync<Exception>().WithMessage("Database Error");
        }

        [Fact]
        public async Task GetByRecordIdAsync_ShouldCallRepositoryOnce()
        {
            var record = GetHealthRecord();
            var dto = GetHealthRecordDto();

            _healthRecordRepository.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(record);

            _mapper.Setup(x => x.Map<HealthRecordDto>(record)).Returns(dto);

            await _service.GetByRecordIdAsync(1);

            _healthRecordRepository.Verify(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetByRecordIdAsync_ShouldCallMapperOnce()
        {
            var record = GetHealthRecord();
            var dto = GetHealthRecordDto();

            _healthRecordRepository.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(record);

            _mapper.Setup(x => x.Map<HealthRecordDto>(record)).Returns(dto);

            await _service.GetByRecordIdAsync(1);

            _mapper.Verify(x => x.Map<HealthRecordDto>(record), Times.Once);
        }

        [Fact]
        public async Task GetByRecordIdAsync_ShouldPassCancellationToken()
        {
            var token = new CancellationToken();
            var record = GetHealthRecord();
            var dto = GetHealthRecordDto();

            _healthRecordRepository.Setup(x => x.GetByIdAsync(1, token)).ReturnsAsync(record);
            _mapper.Setup(x => x.Map<HealthRecordDto>(record)).Returns(dto);

            await _service.GetByRecordIdAsync(1, token);

            _healthRecordRepository.Verify(x => x.GetByIdAsync(1, token), Times.Once);
        }

        [Fact]
        public async Task AddAsync_ShouldCreateHealthRecord()
        {
            var dto = GetCreateHealthRecordDto();
            var appointment = GetAppointment(AppointmentStatus.Completed);
            var savedRecord = GetHealthRecord();
            var response = GetHealthRecordDto();

            _appointmentRepository.Setup(x => x.GetByIdAsync(dto.AppointmentId, It.IsAny<CancellationToken>())).ReturnsAsync(appointment);
            _healthRecordRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<HealthRecord>());
            _healthRecordRepository.Setup(x => x.AddAsync(It.IsAny<HealthRecord>(), It.IsAny<CancellationToken>())).ReturnsAsync(savedRecord);
            _mapper.Setup(x => x.Map<HealthRecordDto>(savedRecord)).Returns(response);

            var result = await _service.AddAsync(dto);

            result.Should().NotBeNull();
            result.RecordId.Should().Be(1);
            result.Diagnosis.Should().Be("Fever");
        }

        [Fact]
        public async Task AddAsync_ShouldThrowNotFoundException_WhenAppointmentDoesNotExist()
        {
            var dto = GetCreateHealthRecordDto();

            _appointmentRepository.Setup(x => x.GetByIdAsync(dto.AppointmentId, It.IsAny<CancellationToken>())).ReturnsAsync((Appointment?)null);

            Func<Task> action = async () => await _service.AddAsync(dto);

            await action.Should().ThrowAsync<NotFoundException>().WithMessage("Appointment not found.");
        }

        [Fact]
        public async Task AddAsync_ShouldThrowValidationException_WhenAppointmentNotCompleted()
        {
            var dto = GetCreateHealthRecordDto();
            var appointment = GetAppointment(AppointmentStatus.Pending);

            _appointmentRepository.Setup(x => x.GetByIdAsync(dto.AppointmentId, It.IsAny<CancellationToken>())).ReturnsAsync(appointment);

            Func<Task> action = async () => await _service.AddAsync(dto);

            await action.Should().ThrowAsync<ValidationException>().WithMessage("Health records can only be created for completed appointments.");
        }

        [Fact]
        public async Task AddAsync_ShouldThrowValidationException_WhenHealthRecordAlreadyExists()
        {
            var dto = GetCreateHealthRecordDto();
            var appointment = GetAppointment(AppointmentStatus.Completed);

            var existingRecords = new List<HealthRecord>
            {
                new HealthRecord { AppointmentId = dto.AppointmentId }
            };

            _appointmentRepository.Setup(x => x.GetByIdAsync(dto.AppointmentId, It.IsAny<CancellationToken>())).ReturnsAsync(appointment);
            _healthRecordRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(existingRecords);

            Func<Task> action = async () => await _service.AddAsync(dto);

            await action.Should().ThrowAsync<ValidationException>().WithMessage("Health record already exists for this appointment.");
        }

        [Fact]
        public async Task AddAsync_ShouldCallRepositoryAddOnce()
        {
            var dto = GetCreateHealthRecordDto();
            var appointment = GetAppointment(AppointmentStatus.Completed);
            var record = GetHealthRecord();

            _appointmentRepository.Setup(x => x.GetByIdAsync(dto.AppointmentId, It.IsAny<CancellationToken>())).ReturnsAsync(appointment);
            _healthRecordRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<HealthRecord>());
            _healthRecordRepository.Setup(x => x.AddAsync(It.IsAny<HealthRecord>(), It.IsAny<CancellationToken>())).ReturnsAsync(record);
            _mapper.Setup(x => x.Map<HealthRecordDto>(record)).Returns(GetHealthRecordDto());

            await _service.AddAsync(dto);

            _healthRecordRepository.Verify(x => x.AddAsync(It.IsAny<HealthRecord>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        //[Fact]
        //public async Task GetByPatientIdAsync_ShouldThrowNotFound_WhenNoRecords()
        //{
        //    _healthRecordRepository.Setup(x => x.GetByPatientIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(new List<HealthRecord>());

        //    Func<Task> action = async () => await _service.GetByPatientIdAsync(5);

        //    await action.Should().ThrowAsync<NotFoundException>().WithMessage("No health records found for patient with ID 5.");
        //}

        //[Fact]
        //public async Task GetByPatientIdAsync_ShouldReturnMapped_WhenRecordsExist()
        //{
        //    var records = new List<HealthRecord> { GetHealthRecord() };
        //    _healthRecordRepository.Setup(x => x.GetByPatientIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(records);
        //    _mapper.Setup(x => x.Map<IEnumerable<HealthRecordDto>>(records)).Returns(new List<HealthRecordDto> { GetHealthRecordDto() });

        //    var result = await _service.GetByPatientIdAsync(1);

        //    result.Should().HaveCount(1);
        //}
    }
}
