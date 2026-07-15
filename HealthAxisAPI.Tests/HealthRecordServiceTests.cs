using AutoMapper;
using FluentAssertions;
using HealthAxis.Shared.DTOs.HealthRecordDtos;
using HealthAxis.API.Exceptions;
using HealthAxis.API.Models;
using HealthAxis.API.Repositories.Interfaces;
using HealthAxis.API.Services.Implementations;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace HealthAxis.Tests.Services
{
    public class HealthRecordServiceTests
    {
        private readonly Mock<IHealthRecordRepository> _healthRecordRepository;
        private readonly Mock<IMapper> _mapper;

        private readonly HealthRecordService _service;

        public HealthRecordServiceTests()
        {
            _healthRecordRepository = new Mock<IHealthRecordRepository>();
            _mapper = new Mock<IMapper>();

            _service = new HealthRecordService(
                _healthRecordRepository.Object,
                _mapper.Object);
        }

        private static HealthRecord GetHealthRecord()
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

        private static HealthRecordDto GetHealthRecordDto()
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
        public async Task GetByRecordIdAsync_ShouldPropagateRepositoryException()
        {
            _healthRecordRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Database Error"));

            Func<Task> action = async () => await _service.GetByRecordIdAsync(1);

            await action.Should().ThrowAsync<Exception>().WithMessage("Database Error");
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
    }
}