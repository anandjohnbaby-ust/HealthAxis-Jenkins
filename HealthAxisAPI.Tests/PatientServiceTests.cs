//using AutoMapper;
//using FluentAssertions;
//using HealthAxis.API.Exceptions;
//using HealthAxis.API.Models;
//using HealthAxis.API.Repositories.Interfaces;
//using HealthAxis.API.Services.Implementations;
//using HealthAxis.Shared.Common;
//using HealthAxis.Shared.DTOs.AppointmentDtos;
//using HealthAxis.Shared.DTOs.DoctorDtos;
//using HealthAxis.Shared.DTOs.HealthRecordDtos;
//using HealthAxis.Shared.DTOs.PatientDtos;
//using HealthAxis.Shared.Enums;
//using Moq;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;
//using Xunit;

//namespace HealthAxis.Tests.Services
//{
//    public class PatientServiceTests
//    {
//        private readonly Mock<IPatientRepository> _patientRepository;
//        private readonly Mock<HealthAxis.API.Services.Interfaces.IAppointmentService> _appointmentService;
//        private readonly Mock<HealthAxis.API.Services.Interfaces.IDoctorService> _doctorService;
//        private readonly Mock<IMapper> _mapper;

//        private readonly PatientService _service;

//        public PatientServiceTests()
//        {
//            _patientRepository = new Mock<IPatientRepository>();
//            _appointmentService = new Mock<HealthAxis.API.Services.Interfaces.IAppointmentService>();
//            _doctorService = new Mock<HealthAxis.API.Services.Interfaces.IDoctorService>();
//            _mapper = new Mock<IMapper>();

//            _service = new PatientService(
//                _patientRepository.Object,
//                _appointmentService.Object,
//                _doctorService.Object,
//                _mapper.Object);
//        }

//        private Patient GetPatient()
//        {
//            return new Patient { PatientId = 1, FullName = "John" };
//        }

//        private PatientDto GetPatientDto()
//        {
//            return new PatientDto { PatientId = 1, FullName = "John" };
//        }

//        // =====================================================================
//        // GetByIdAsync
//        // =====================================================================

//        [Fact]
//        public async Task GetByIdAsync_ShouldReturnMapped_WhenFound()
//        {
//            var patient = GetPatient();
//            var dto = GetPatientDto();

//            _patientRepository.Setup(p => p.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(patient);
//            _mapper.Setup(m => m.Map<PatientDto>(patient)).Returns(dto);

//            var result = await _service.GetByIdAsync(1);

//            result.Should().NotBeNull();
//            result.PatientId.Should().Be(1);
//            _patientRepository.Verify(p => p.GetByIdAsync(1, It.IsAny<CancellationToken>()), Times.Once);
//            _mapper.Verify(m => m.Map<PatientDto>(patient), Times.Once);
//        }

//        [Fact]
//        public async Task GetByIdAsync_ShouldThrowNotFound_WhenMissing()
//        {
//            _patientRepository.Setup(p => p.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync((Patient?)null);

//            Func<Task> action = async () => await _service.GetByIdAsync(5);

//            await action.Should().ThrowAsync<NotFoundException>().WithMessage("Patient not found.");
//        }

//        [Fact]
//        public async Task GetByIdAsync_ShouldPropagateRepositoryException()
//        {
//            _patientRepository.Setup(p => p.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("db"));

//            Func<Task> action = async () => await _service.GetByIdAsync(1);

//            await action.Should().ThrowAsync<Exception>().WithMessage("db");
//        }

//        [Fact]
//        public async Task GetByIdAsync_ShouldForwardCancellationToken()
//        {
//            using var cts = new CancellationTokenSource();
//            var patient = new Patient { PatientId = 1 };

//            _patientRepository
//                .Setup(p => p.GetByIdAsync(1, cts.Token))
//                .ReturnsAsync(patient);
//            _mapper.Setup(m => m.Map<PatientDto>(patient)).Returns(new PatientDto { PatientId = 1 });

//            await _service.GetByIdAsync(1, cts.Token);

//            _patientRepository.Verify(p => p.GetByIdAsync(1, cts.Token), Times.Once);
//        }

//        // =====================================================================
//        // GetPatientsAsync
//        // =====================================================================

//        [Fact]
//        public async Task GetPatientsAsync_ShouldReturnPagedMapped()
//        {
//            var req = new PaginationRequest { PageNumber = 1, PageSize = 10 };
//            var patients = new List<Patient> { new Patient { PatientId = 1 }, new Patient { PatientId = 2 } };
//            var paged = new PagedResult<Patient> { Items = patients, TotalCount = 2, PageNumber = 1, PageSize = 10 };
//            var dtos = new List<PatientDto> { new PatientDto { PatientId = 1 }, new PatientDto { PatientId = 2 } };

//            _patientRepository.Setup(r => r.GetPatientsAsync(req, "x", It.IsAny<CancellationToken>())).ReturnsAsync(paged);
//            _mapper.Setup(m => m.Map<IEnumerable<PatientDto>>(patients)).Returns(dtos);

//            var result = await _service.GetPatientsAsync(req, "x");

//            result.Should().NotBeNull();
//            result.TotalCount.Should().Be(2);
//            result.Items.Should().HaveCount(2);
//        }

//        [Fact]
//        public async Task GetPatientsAsync_ShouldPropagateException()
//        {
//            var req = new PaginationRequest();
//            _patientRepository.Setup(r => r.GetPatientsAsync(req, null, It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("fail"));

//            Func<Task> action = async () => await _service.GetPatientsAsync(req, null);
//            await action.Should().ThrowAsync<Exception>().WithMessage("fail");
//        }

//        [Fact]
//        public async Task GetPatientsAsync_ShouldHandleNullSearch()
//        {
//            var req = new PaginationRequest { PageNumber = 1, PageSize = 5 };
//            var paged = new PagedResult<Patient>
//            {
//                Items = new List<Patient>(),
//                TotalCount = 0,
//                PageNumber = 1,
//                PageSize = 5
//            };

//            _patientRepository
//                .Setup(r => r.GetPatientsAsync(req, null, It.IsAny<CancellationToken>()))
//                .ReturnsAsync(paged);
//            _mapper
//                .Setup(m => m.Map<IEnumerable<PatientDto>>(paged.Items))
//                .Returns(new List<PatientDto>());

//            var result = await _service.GetPatientsAsync(req, null);

//            result.Items.Should().BeEmpty();
//            result.TotalCount.Should().Be(0);
//        }

//        [Fact]
//        public async Task GetPatientsAsync_ShouldReturnEmptyPage_WhenNoMatches()
//        {
//            var req = new PaginationRequest { PageNumber = 2, PageSize = 10 };
//            var paged = new PagedResult<Patient>
//            {
//                Items = new List<Patient>(),
//                TotalCount = 0,
//                PageNumber = 2,
//                PageSize = 10
//            };

//            _patientRepository
//                .Setup(r => r.GetPatientsAsync(req, "zzz", It.IsAny<CancellationToken>()))
//                .ReturnsAsync(paged);
//            _mapper
//                .Setup(m => m.Map<IEnumerable<PatientDto>>(paged.Items))
//                .Returns(new List<PatientDto>());

//            var result = await _service.GetPatientsAsync(req, "zzz");

//            result.Items.Should().BeEmpty();
//            result.PageNumber.Should().Be(2);
//            result.PageSize.Should().Be(10);
//        }

//        // =====================================================================
//        // UpdateAsync
//        // =====================================================================

//        [Fact]
//        public async Task UpdateAsync_ShouldThrowNotFound_WhenMissing()
//        {
//            _patientRepository.Setup(p => p.GetByIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync((Patient?)null);

//            Func<Task> action = async () => await _service.UpdateAsync(10, new UpdatePatientDto());

//            await action.Should().ThrowAsync<NotFoundException>().WithMessage("Patient not found.");
//        }

//        [Fact]
//        public async Task UpdateAsync_ShouldMapAndUpdate_WhenExists()
//        {
//            var patient = GetPatient();
//            var dto = new UpdatePatientDto { FullName = "New" };

//            _patientRepository.Setup(p => p.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(patient);
//            _patientRepository.Setup(p => p.UpdateAsync(1, patient, It.IsAny<CancellationToken>())).ReturnsAsync(patient);
//            _mapper.Setup(m => m.Map<PatientDto>(patient)).Returns(GetPatientDto());

//            var result = await _service.UpdateAsync(1, dto);

//            _mapper.Verify(m => m.Map(dto, patient), Times.Once);
//            _patientRepository.Verify(p => p.UpdateAsync(1, patient, It.IsAny<CancellationToken>()), Times.Once);
//            result.PatientId.Should().Be(1);
//        }

//        [Fact]
//        public async Task UpdateAsync_ShouldPropagateException_WhenGetByIdFails()
//        {
//            _patientRepository
//                .Setup(p => p.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
//                .ThrowsAsync(new Exception("lookup failed"));

//            Func<Task> action = async () => await _service.UpdateAsync(1, new UpdatePatientDto());

//            await action.Should().ThrowAsync<Exception>().WithMessage("lookup failed");
//        }

//        [Fact]
//        public async Task UpdateAsync_ShouldPropagateException_WhenRepositoryUpdateFails()
//        {
//            var patient = new Patient { PatientId = 1 };

//            _patientRepository
//                .Setup(p => p.GetByIdAsync(1, It.IsAny<CancellationToken>()))
//                .ReturnsAsync(patient);
//            _patientRepository
//                .Setup(p => p.UpdateAsync(1, patient, It.IsAny<CancellationToken>()))
//                .ThrowsAsync(new Exception("update failed"));

//            Func<Task> action = async () => await _service.UpdateAsync(1, new UpdatePatientDto());

//            await action.Should().ThrowAsync<Exception>().WithMessage("update failed");
//        }

//        // =====================================================================
//        // GetHealthRecordsByPatientId
//        // =====================================================================

//        [Fact]
//        public async Task GetHealthRecordsByPatientId_ShouldThrow_WhenPatientNotFound()
//        {
//            _patientRepository.Setup(p => p.GetHealthRecordsByPatientId(5)).ReturnsAsync((Patient?)null);

//            Func<Task> action = async () => await _service.GetHealthRecordsByPatientId(5);

//            await action.Should().ThrowAsync<NotFoundException>().WithMessage("Patient with ID 5 not found");
//        }

//        [Fact]
//        public async Task GetHealthRecordsByPatientId_ShouldReturnMapped_WhenFound()
//        {
//            var patient = new Patient { PatientId = 2, HealthRecords = new List<HealthRecord> { new HealthRecord { RecordId = 1 } } };
//            var dto = new HealthRecordDto { RecordId = 1 };

//            _patientRepository.Setup(p => p.GetHealthRecordsByPatientId(2)).ReturnsAsync(patient);
//            _mapper.Setup(m => m.Map<IEnumerable<HealthRecordDto>>(patient.HealthRecords)).Returns(new List<HealthRecordDto> { dto });

//            var result = await _service.GetHealthRecordsByPatientId(2);

//            result.Should().HaveCount(1);
//        }

//        [Fact]
//        public async Task GetHealthRecordsByPatientId_ShouldReturnEmpty_WhenPatientHasNoRecords()
//        {
//            var patient = new Patient { PatientId = 3, HealthRecords = new List<HealthRecord>() };

//            _patientRepository.Setup(p => p.GetHealthRecordsByPatientId(3)).ReturnsAsync(patient);
//            _mapper
//                .Setup(m => m.Map<IEnumerable<HealthRecordDto>>(patient.HealthRecords))
//                .Returns(new List<HealthRecordDto>());

//            var result = await _service.GetHealthRecordsByPatientId(3);

//            result.Should().BeEmpty();
//        }

//        [Fact]
//        public async Task GetHealthRecordsByPatientId_ShouldPropagateRepositoryException()
//        {
//            _patientRepository
//                .Setup(p => p.GetHealthRecordsByPatientId(It.IsAny<int>()))
//                .ThrowsAsync(new Exception("db error"));

//            Func<Task> action = async () => await _service.GetHealthRecordsByPatientId(4);

//            await action.Should().ThrowAsync<Exception>().WithMessage("db error");
//        }

//        // =====================================================================
//        // BookAppointmentAsync
//        // =====================================================================

//        [Fact]
//        public async Task BookAppointmentAsync_ForwardsToAppointmentService()
//        {
//            var dto = new CreateAppointmentDto { DoctorId = 1, PatientId = 2 };
//            var ret = new AppointmentDto { AppointmentId = 5 };

//            _appointmentService.Setup(a => a.BookAppointmentAsync(dto, It.IsAny<CancellationToken>())).ReturnsAsync(ret);

//            var result = await _service.BookAppointmentAsync(dto);

//            result.Should().Be(ret);
//            _appointmentService.Verify(a => a.BookAppointmentAsync(dto, It.IsAny<CancellationToken>()), Times.Once);
//        }

//        [Fact]
//        public async Task BookAppointmentAsync_ShouldPropagateException_FromAppointmentService()
//        {
//            var dto = new CreateAppointmentDto { DoctorId = 1, PatientId = 2 };

//            _appointmentService
//                .Setup(a => a.BookAppointmentAsync(dto, It.IsAny<CancellationToken>()))
//                .ThrowsAsync(new Exception("slot unavailable"));

//            Func<Task> action = async () => await _service.BookAppointmentAsync(dto);

//            await action.Should().ThrowAsync<Exception>().WithMessage("slot unavailable");
//        }

//        // =====================================================================
//        // GetAvailableDoctorsAsync
//        // =====================================================================

//        [Fact]
//        public async Task GetAvailableDoctorsAsync_ForwardsToDoctorService()
//        {
//            var list = new List<DoctorDto> { new DoctorDto { DoctorId = 1 } };
//            _doctorService.Setup(d => d.GetAvailableDoctorsAsync(null, null, It.IsAny<CancellationToken>())).ReturnsAsync(list);

//            var result = await _service.GetAvailableDoctorsAsync(null, null);

//            result.Should().HaveCount(1);
//        }

//        [Fact]
//        public async Task GetAvailableDoctorsAsync_ShouldForwardSpecialisationAndSearch()
//        {
//            var list = new List<DoctorDto> { new DoctorDto { DoctorId = 2 } };

//            _doctorService
//                .Setup(d => d.GetAvailableDoctorsAsync(Specialisation.Cardiology, "smith", It.IsAny<CancellationToken>()))
//                .ReturnsAsync(list);

//            var result = await _service.GetAvailableDoctorsAsync(Specialisation.Cardiology, "smith");

//            result.Should().HaveCount(1);
//            _doctorService.Verify(
//                d => d.GetAvailableDoctorsAsync(Specialisation.Cardiology, "smith", It.IsAny<CancellationToken>()),
//                Times.Once);
//        }

//        [Fact]
//        public async Task GetAvailableDoctorsAsync_ShouldReturnEmpty_WhenNoneAvailable()
//        {
//            _doctorService
//                .Setup(d => d.GetAvailableDoctorsAsync(It.IsAny<Specialisation?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
//                .ReturnsAsync(new List<DoctorDto>());

//            var result = await _service.GetAvailableDoctorsAsync(Specialisation.Dermatology, "nomatch");

//            result.Should().BeEmpty();
//        }

//        [Fact]
//        public async Task GetAvailableDoctorsAsync_ShouldPropagateException()
//        {
//            _doctorService
//                .Setup(d => d.GetAvailableDoctorsAsync(It.IsAny<Specialisation?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
//                .ThrowsAsync(new Exception("doctor service down"));

//            Func<Task> action = async () => await _service.GetAvailableDoctorsAsync(null, null);

//            await action.Should().ThrowAsync<Exception>().WithMessage("doctor service down");
//        }

//        // =====================================================================
//        // GetAppointmentsByPatientIdAsync
//        // =====================================================================

//        [Fact]
//        public async Task GetAppointmentsByPatientIdAsync_ShouldThrow_WhenPatientMissing()
//        {
//            _patientRepository.Setup(p => p.GetByIdAsync(7, It.IsAny<CancellationToken>())).ReturnsAsync((Patient?)null);

//            Func<Task> action = async () => await _service.GetAppointmentsByPatientIdAsync(7);

//            await action.Should().ThrowAsync<NotFoundException>().WithMessage("Patient not found.");
//        }

//        [Fact]
//        public async Task GetAppointmentsByPatientIdAsync_ShouldReturnMapped_WhenExists()
//        {
//            var patient = new Patient { PatientId = 8 };
//            var appts = new List<Appointment> { new Appointment { AppointmentId = 11 } };

//            _patientRepository.Setup(p => p.GetByIdAsync(8, It.IsAny<CancellationToken>())).ReturnsAsync(patient);
//            _patientRepository.Setup(p => p.GetAppointmentsByPatientIdAsync(8, It.IsAny<CancellationToken>())).ReturnsAsync(appts);
//            _mapper.Setup(m => m.Map<IEnumerable<AppointmentDto>>(appts)).Returns(new List<AppointmentDto> { new AppointmentDto { AppointmentId = 11 } });

//            var result = await _service.GetAppointmentsByPatientIdAsync(8);

//            result.Should().HaveCount(1);
//        }

//        [Fact]
//        public async Task GetAppointmentsByPatientIdAsync_ShouldReturnEmpty_WhenNoAppointments()
//        {
//            var patient = new Patient { PatientId = 9 };

//            _patientRepository.Setup(p => p.GetByIdAsync(9, It.IsAny<CancellationToken>())).ReturnsAsync(patient);
//            _patientRepository
//                .Setup(p => p.GetAppointmentsByPatientIdAsync(9, It.IsAny<CancellationToken>()))
//                .ReturnsAsync(new List<Appointment>());
//            _mapper
//                .Setup(m => m.Map<IEnumerable<AppointmentDto>>(It.IsAny<IEnumerable<Appointment>>()))
//                .Returns(new List<AppointmentDto>());

//            var result = await _service.GetAppointmentsByPatientIdAsync(9);

//            result.Should().BeEmpty();
//        }

//        [Fact]
//        public async Task GetAppointmentsByPatientIdAsync_ShouldPropagateException_WhenRepositoryFails()
//        {
//            var patient = new Patient { PatientId = 10 };

//            _patientRepository.Setup(p => p.GetByIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(patient);
//            _patientRepository
//                .Setup(p => p.GetAppointmentsByPatientIdAsync(10, It.IsAny<CancellationToken>()))
//                .ThrowsAsync(new Exception("appointments lookup failed"));

//            Func<Task> action = async () => await _service.GetAppointmentsByPatientIdAsync(10);

//            await action.Should().ThrowAsync<Exception>().WithMessage("appointments lookup failed");
//        }

//        // =====================================================================
//        // CancelAppointmentByPatientAsync
//        // =====================================================================

//        [Fact]
//        public async Task CancelAppointmentByPatientAsync_ForwardsToAppointmentService()
//        {
//            var dto = new CancelAppointmentDto { CancellationReason = "s" };
//            var ret = new AppointmentDto { AppointmentId = 20 };

//            _appointmentService.Setup(a => a.CancelAppointmentByPatientAsync(2, 20, dto, It.IsAny<CancellationToken>())).ReturnsAsync(ret);

//            var result = await _service.CancelAppointmentByPatientAsync(2, 20, dto);

//            result.Should().Be(ret);
//            _appointmentService.Verify(a => a.CancelAppointmentByPatientAsync(2, 20, dto, It.IsAny<CancellationToken>()), Times.Once);
//        }

//        [Fact]
//        public async Task CancelAppointmentByPatientAsync_ShouldPropagateException()
//        {
//            var dto = new CancelAppointmentDto { CancellationReason = "changed mind" };

//            _appointmentService
//                .Setup(a => a.CancelAppointmentByPatientAsync(2, 20, dto, It.IsAny<CancellationToken>()))
//                .ThrowsAsync(new NotFoundException("Appointment not found."));

//            Func<Task> action = async () => await _service.CancelAppointmentByPatientAsync(2, 20, dto);

//            await action.Should().ThrowAsync<NotFoundException>().WithMessage("Appointment not found.");
//        }

//        // =====================================================================
//        // GetDashboardAsync
//        // =====================================================================

//        [Fact]
//        public async Task GetDashboardAsync_ShouldThrow_WhenNull()
//        {
//            _patientRepository.Setup(p => p.GetDashboardAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((PatientDashboardDto?)null);

//            Func<Task> action = async () => await _service.GetDashboardAsync(1);

//            await action.Should().ThrowAsync<NotFoundException>().WithMessage("Patient not found.");
//        }

//        [Fact]
//        public async Task GetDashboardAsync_ShouldReturnDashboard_WhenExists()
//        {
//            var dash = new PatientDashboardDto();
//            _patientRepository.Setup(p => p.GetDashboardAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(dash);

//            var result = await _service.GetDashboardAsync(2);

//            result.Should().Be(dash);
//        }

//        [Fact]
//        public async Task GetDashboardAsync_ShouldPropagateException()
//        {
//            _patientRepository
//                .Setup(p => p.GetDashboardAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
//                .ThrowsAsync(new Exception("dashboard aggregation failed"));

//            Func<Task> action = async () => await _service.GetDashboardAsync(1);

//            await action.Should().ThrowAsync<Exception>().WithMessage("dashboard aggregation failed");
//        }

//        [Fact]
//        public async Task GetDashboardAsync_ShouldForwardCancellationToken()
//        {
//            using var cts = new CancellationTokenSource();
//            var dash = new PatientDashboardDto();

//            _patientRepository
//                .Setup(p => p.GetDashboardAsync(3, cts.Token))
//                .ReturnsAsync(dash);

//            await _service.GetDashboardAsync(3, cts.Token);

//            _patientRepository.Verify(p => p.GetDashboardAsync(3, cts.Token), Times.Once);
//        }
//    }
//}