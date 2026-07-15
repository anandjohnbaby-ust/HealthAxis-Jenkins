using AutoMapper;
using FluentAssertions;
using HealthAxis.API.Exceptions;
using HealthAxis.API.Models;
using HealthAxis.API.Repositories.Interfaces;
using HealthAxis.API.Services.Implementations;
using HealthAxis.Shared.Common;
using HealthAxis.Shared.DTOs.AppointmentDtos;
using HealthAxis.Shared.DTOs.HealthRecordDtos;
using HealthAxis.Shared.DTOs.PatientDtos;
using HealthAxis.Shared.Enums;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace HealthAxis.Tests.Services
{
    public class PatientServiceTests
    {
        private readonly Mock<IPatientRepository> _patientRepository;
        private readonly Mock<IMapper> _mapper;

        private readonly PatientService _service;

        public PatientServiceTests()
        {
            _patientRepository = new Mock<IPatientRepository>();
            _mapper = new Mock<IMapper>();

            _service = new PatientService(
                _patientRepository.Object,
                _mapper.Object);
        }

        private static Patient GetPatient()
        {
            return new Patient { PatientId = 1, FullName = "John" };
        }

        private static PatientDto GetPatientDto()
        {
            return new PatientDto { PatientId = 1, FullName = "John" };
        }

        // =====================================================================
        // GetByIdAsync
        // =====================================================================

        [Fact]
        public async Task GetByIdAsync_ShouldReturnMapped_WhenFound()
        {
            var patient = GetPatient();
            var dto = GetPatientDto();

            _patientRepository.Setup(p => p.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(patient);
            _mapper.Setup(m => m.Map<PatientDto>(patient)).Returns(dto);

            var result = await _service.GetByIdAsync(1);

            result.Should().NotBeNull();
            result.PatientId.Should().Be(1);
            _patientRepository.Verify(p => p.GetByIdAsync(1, It.IsAny<CancellationToken>()), Times.Once);
            _mapper.Verify(m => m.Map<PatientDto>(patient), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldThrowNotFound_WhenMissing()
        {
            _patientRepository.Setup(p => p.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync((Patient?)null);

            Func<Task> action = async () => await _service.GetByIdAsync(5);

            await action.Should().ThrowAsync<NotFoundException>().WithMessage("Patient not found.");
        }

        [Fact]
        public async Task GetByIdAsync_ShouldPropagateRepositoryException()
        {
            _patientRepository.Setup(p => p.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("db"));

            Func<Task> action = async () => await _service.GetByIdAsync(1);

            await action.Should().ThrowAsync<Exception>().WithMessage("db");
        }

        [Fact]
        public async Task GetByIdAsync_ShouldForwardCancellationToken()
        {
            using var cts = new CancellationTokenSource();
            var patient = new Patient { PatientId = 1 };

            _patientRepository
                .Setup(p => p.GetByIdAsync(1, cts.Token))
                .ReturnsAsync(patient);
            _mapper.Setup(m => m.Map<PatientDto>(patient)).Returns(new PatientDto { PatientId = 1 });

            await _service.GetByIdAsync(1, cts.Token);

            _patientRepository.Verify(p => p.GetByIdAsync(1, cts.Token), Times.Once);
        }

        // =====================================================================
        // GetPatientsAsync
        // =====================================================================

        [Fact]
        public async Task GetPatientsAsync_ShouldReturnPagedMapped()
        {
            var req = new PaginationRequest { PageNumber = 1, PageSize = 10 };
            var patients = new List<Patient> { new Patient { PatientId = 1 }, new Patient { PatientId = 2 } };
            var paged = new PagedResult<Patient> { Items = patients, TotalCount = 2, PageNumber = 1, PageSize = 10 };
            var dtos = new List<PatientDto> { new PatientDto { PatientId = 1 }, new PatientDto { PatientId = 2 } };

            _patientRepository.Setup(r => r.GetPatientsAsync(req, "x", It.IsAny<CancellationToken>())).ReturnsAsync(paged);
            _mapper.Setup(m => m.Map<IEnumerable<PatientDto>>(patients)).Returns(dtos);

            var result = await _service.GetPatientsAsync(req, "x");

            result.Should().NotBeNull();
            result.TotalCount.Should().Be(2);
            result.Items.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetPatientsAsync_ShouldPropagateException()
        {
            var req = new PaginationRequest();
            _patientRepository.Setup(r => r.GetPatientsAsync(req, null, It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("fail"));

            Func<Task> action = async () => await _service.GetPatientsAsync(req, null);
            await action.Should().ThrowAsync<Exception>().WithMessage("fail");
        }

        [Fact]
        public async Task GetPatientsAsync_ShouldHandleNullSearch()
        {
            var req = new PaginationRequest { PageNumber = 1, PageSize = 5 };
            var paged = new PagedResult<Patient>
            {
                Items = new List<Patient>(),
                TotalCount = 0,
                PageNumber = 1,
                PageSize = 5
            };

            _patientRepository
                .Setup(r => r.GetPatientsAsync(req, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(paged);
            _mapper
                .Setup(m => m.Map<IEnumerable<PatientDto>>(paged.Items))
                .Returns(new List<PatientDto>());

            var result = await _service.GetPatientsAsync(req, null);

            result.Items.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task GetPatientsAsync_ShouldReturnEmptyPage_WhenNoMatches()
        {
            var req = new PaginationRequest { PageNumber = 2, PageSize = 10 };
            var paged = new PagedResult<Patient>
            {
                Items = new List<Patient>(),
                TotalCount = 0,
                PageNumber = 2,
                PageSize = 10
            };

            _patientRepository
                .Setup(r => r.GetPatientsAsync(req, "zzz", It.IsAny<CancellationToken>()))
                .ReturnsAsync(paged);
            _mapper
                .Setup(m => m.Map<IEnumerable<PatientDto>>(paged.Items))
                .Returns(new List<PatientDto>());

            var result = await _service.GetPatientsAsync(req, "zzz");

            result.Items.Should().BeEmpty();
            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(10);
        }

        // =====================================================================
        // UpdateAsync
        // =====================================================================

        [Fact]
        public async Task UpdateAsync_ShouldThrowNotFound_WhenMissing()
        {
            _patientRepository.Setup(p => p.GetByIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync((Patient?)null);

            Func<Task> action = async () => await _service.UpdateAsync(10, new UpdatePatientDto());

            await action.Should().ThrowAsync<NotFoundException>().WithMessage("Patient not found.");
        }

        [Fact]
        public async Task UpdateAsync_ShouldMapAndUpdate_WhenExists()
        {
            var patient = GetPatient();
            var dto = new UpdatePatientDto { FullName = "New" };

            _patientRepository.Setup(p => p.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(patient);
            _patientRepository.Setup(p => p.UpdateAsync(1, patient, It.IsAny<CancellationToken>())).ReturnsAsync(patient);
            _mapper.Setup(m => m.Map<PatientDto>(patient)).Returns(GetPatientDto());

            var result = await _service.UpdateAsync(1, dto);

            _mapper.Verify(m => m.Map(dto, patient), Times.Once);
            _patientRepository.Verify(p => p.UpdateAsync(1, patient, It.IsAny<CancellationToken>()), Times.Once);
            result.PatientId.Should().Be(1);
        }

        [Fact]
        public async Task UpdateAsync_ShouldPropagateException_WhenGetByIdFails()
        {
            _patientRepository
                .Setup(p => p.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("lookup failed"));

            Func<Task> action = async () => await _service.UpdateAsync(1, new UpdatePatientDto());

            await action.Should().ThrowAsync<Exception>().WithMessage("lookup failed");
        }

        [Fact]
        public async Task UpdateAsync_ShouldPropagateException_WhenRepositoryUpdateFails()
        {
            var patient = new Patient { PatientId = 1 };

            _patientRepository
                .Setup(p => p.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(patient);
            _patientRepository
                .Setup(p => p.UpdateAsync(1, patient, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("update failed"));

            Func<Task> action = async () => await _service.UpdateAsync(1, new UpdatePatientDto());

            await action.Should().ThrowAsync<Exception>().WithMessage("update failed");
        }

        // =====================================================================
        // GetHealthRecordsByPatientId
        // =====================================================================

        [Fact]
        public async Task GetHealthRecordsByPatientId_ShouldReturnPagedMapped()
        {
            var request = new PaginationRequest { PageNumber = 1, PageSize = 10 };
            var paged = new PagedResult<HealthRecord>
            {
                Items = new List<HealthRecord> { new HealthRecord { RecordId = 1 } },
                TotalCount = 1,
                PageNumber = 1,
                PageSize = 10
            };

            _patientRepository
                .Setup(p => p.GetHealthRecordsByPatientIdAsync(2, It.IsAny<PaginationRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(paged);

            _mapper.Setup(m => m.Map<IEnumerable<HealthRecordDto>>(paged.Items))
                .Returns(new List<HealthRecordDto> { new HealthRecordDto { RecordId = 1 } });

            var result = await _service.GetHealthRecordsByPatientId(2, request);

            result.Items.Should().HaveCount(1);
            result.TotalCount.Should().Be(1);
        }

        [Fact]
        public async Task GetHealthRecordsByPatientId_ShouldReturnEmpty_WhenNoRecords()
        {
            var request = new PaginationRequest { PageNumber = 1, PageSize = 10 };
            var paged = new PagedResult<HealthRecord>
            {
                Items = new List<HealthRecord>(),
                TotalCount = 0,
                PageNumber = 1,
                PageSize = 10
            };

            _patientRepository
                .Setup(p => p.GetHealthRecordsByPatientIdAsync(3, It.IsAny<PaginationRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(paged);

            _mapper.Setup(m => m.Map<IEnumerable<HealthRecordDto>>(paged.Items))
                .Returns(new List<HealthRecordDto>());

            var result = await _service.GetHealthRecordsByPatientId(3, request);

            result.Items.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task GetHealthRecordsByPatientId_ShouldPropagateRepositoryException()
        {
            _patientRepository
                .Setup(p => p.GetHealthRecordsByPatientIdAsync(It.IsAny<int>(), It.IsAny<PaginationRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("db error"));

            Func<Task> action = async () => await _service.GetHealthRecordsByPatientId(4, new PaginationRequest());

            await action.Should().ThrowAsync<Exception>().WithMessage("db error");
        }

        // =====================================================================
        // GetAppointmentsByPatientIdAsync
        // =====================================================================

        [Fact]
        public async Task GetAppointmentsByPatientIdAsync_ShouldThrow_WhenPatientMissing()
        {
            _patientRepository.Setup(p => p.GetByIdAsync(7, It.IsAny<CancellationToken>())).ReturnsAsync((Patient?)null);

            Func<Task> action = async () => await _service.GetAppointmentsByPatientIdAsync(7, new PaginationRequest(), null, null, null);

            await action.Should().ThrowAsync<NotFoundException>().WithMessage("Patient not found.");
        }

        [Fact]
        public async Task GetAppointmentsByPatientIdAsync_ShouldReturnMapped_WhenExists()
        {
            var patient = new Patient { PatientId = 8 };
            var appts = new List<Appointment> { new Appointment { AppointmentId = 11 } };
            var paged = new PagedResult<Appointment> { Items = appts, TotalCount = 1, PageNumber = 1, PageSize = 10 };

            _patientRepository.Setup(p => p.GetByIdAsync(8, It.IsAny<CancellationToken>())).ReturnsAsync(patient);
            _patientRepository.Setup(p => p.GetAppointmentsByPatientIdAsync(8, It.IsAny<PaginationRequest>(), It.IsAny<string?>(), It.IsAny<AppointmentStatus?>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>())).ReturnsAsync(paged);
            _mapper.Setup(m => m.Map<IEnumerable<AppointmentDto>>(appts)).Returns(new List<AppointmentDto> { new AppointmentDto { AppointmentId = 11 } });

            var result = await _service.GetAppointmentsByPatientIdAsync(8, new PaginationRequest(), null, null, null);

            result.Items.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetAppointmentsByPatientIdAsync_ShouldReturnEmpty_WhenNoAppointments()
        {
            var patient = new Patient { PatientId = 9 };
            var paged = new PagedResult<Appointment> { Items = new List<Appointment>(), TotalCount = 0, PageNumber = 1, PageSize = 10 };

            _patientRepository.Setup(p => p.GetByIdAsync(9, It.IsAny<CancellationToken>())).ReturnsAsync(patient);
            _patientRepository.Setup(p => p.GetAppointmentsByPatientIdAsync(9, It.IsAny<PaginationRequest>(), It.IsAny<string?>(), It.IsAny<AppointmentStatus?>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>())).ReturnsAsync(paged);
            _mapper.Setup(m => m.Map<IEnumerable<AppointmentDto>>(It.IsAny<IEnumerable<Appointment>>())).Returns(new List<AppointmentDto>());

            var result = await _service.GetAppointmentsByPatientIdAsync(9, new PaginationRequest(), null, null, null);

            result.Items.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAppointmentsByPatientIdAsync_ShouldPropagateException_WhenRepositoryFails()
        {
            var patient = new Patient { PatientId = 10 };

            _patientRepository.Setup(p => p.GetByIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(patient);
            _patientRepository.Setup(p => p.GetAppointmentsByPatientIdAsync(10, It.IsAny<PaginationRequest>(), It.IsAny<string?>(), It.IsAny<AppointmentStatus?>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("appointments lookup failed"));

            Func<Task> action = async () => await _service.GetAppointmentsByPatientIdAsync(10, new PaginationRequest(), null, null, null);

            await action.Should().ThrowAsync<Exception>().WithMessage("appointments lookup failed");
        }

        // =====================================================================
        // GetDashboardAsync
        // =====================================================================

        [Fact]
        public async Task GetDashboardAsync_ShouldThrow_WhenNull()
        {
            _patientRepository.Setup(p => p.GetDashboardAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((PatientDashboardDto?)null);

            Func<Task> action = async () => await _service.GetDashboardAsync(1);

            await action.Should().ThrowAsync<NotFoundException>().WithMessage("Patient not found.");
        }

        [Fact]
        public async Task GetDashboardAsync_ShouldReturnDashboard_WhenExists()
        {
            var dash = new PatientDashboardDto();
            _patientRepository.Setup(p => p.GetDashboardAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(dash);

            var result = await _service.GetDashboardAsync(2);

            result.Should().Be(dash);
        }

        [Fact]
        public async Task GetDashboardAsync_ShouldPropagateException()
        {
            _patientRepository
                .Setup(p => p.GetDashboardAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("dashboard aggregation failed"));

            Func<Task> action = async () => await _service.GetDashboardAsync(1);

            await action.Should().ThrowAsync<Exception>().WithMessage("dashboard aggregation failed");
        }

        [Fact]
        public async Task GetDashboardAsync_ShouldForwardCancellationToken()
        {
            using var cts = new CancellationTokenSource();
            var dash = new PatientDashboardDto();

            _patientRepository
                .Setup(p => p.GetDashboardAsync(3, cts.Token))
                .ReturnsAsync(dash);

            await _service.GetDashboardAsync(3, cts.Token);

            _patientRepository.Verify(p => p.GetDashboardAsync(3, cts.Token), Times.Once);
        }
    }
}