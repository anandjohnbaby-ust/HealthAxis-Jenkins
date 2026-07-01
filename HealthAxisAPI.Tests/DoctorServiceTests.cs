using AutoMapper;
using FluentAssertions;
using HealthAxis.API.Data;
using HealthAxis.API.Exceptions;
using HealthAxis.API.Models;
using HealthAxis.API.Repositories.Interfaces;
using HealthAxis.API.Services.Implementations;
using HealthAxis.API.Services.Interfaces;
using HealthAxis.Shared.Common;
using HealthAxis.Shared.DTOs.DoctorDtos;
using HealthAxis.Shared.DTOs.AppointmentDtos;
using HealthAxis.Shared.DTOs.HealthRecordDtos;
using HealthAxis.Shared.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace HealthAxis.Tests.Services
{
    public class DoctorServiceTests
    {
        private readonly Mock<IDoctorRepository> _doctorRepository;
        private readonly Mock<IHealthRecordRepository> _healthRecordRepository;
        private readonly Mock<UserManager<ApplicationUser>> _userManager;
        private readonly Mock<ApplicationDbContext> _context;
        private readonly Mock<IMapper> _mapper;
        private readonly Mock<IAppointmentService> _appointmentService;
        private readonly Mock<IHealthRecordService> _healthRecordService;

        private readonly DoctorService _service;

        public DoctorServiceTests()
        {
            _doctorRepository = new Mock<IDoctorRepository>();
            _healthRecordRepository = new Mock<IHealthRecordRepository>();

            var store = new Mock<IUserStore<ApplicationUser>>();

            _userManager = new Mock<UserManager<ApplicationUser>>(
                store.Object,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null);

            _context = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());

            _mapper = new Mock<IMapper>();

            _appointmentService = new Mock<IAppointmentService>();

            _healthRecordService = new Mock<IHealthRecordService>();

            _service = new DoctorService(
                _doctorRepository.Object,
                _healthRecordRepository.Object,
                _userManager.Object,
                _context.Object,
                _mapper.Object,
                _appointmentService.Object,
                _healthRecordService.Object);
        }

        // --- GetDoctorById (existing tests kept) ---
        [Fact]
        public async Task GetDoctorById_ShouldReturnDoctor_WhenDoctorExists()
        {
            int doctorId = 1;
            var doctor = new Doctor { DoctorId = doctorId, FullName = "John Doe" };
            var dto = new DoctorDto { DoctorId = doctorId, FullName = "John Doe" };

            _doctorRepository.Setup(x => x.GetByIdAsync(doctorId)).ReturnsAsync(doctor);
            _mapper.Setup(x => x.Map<DoctorDto>(doctor)).Returns(dto);

            var result = await _service.GetDoctorById(doctorId);

            result.Should().NotBeNull();
            result.DoctorId.Should().Be(doctorId);
            result.FullName.Should().Be("John Doe");
            _doctorRepository.Verify(x => x.GetByIdAsync(doctorId), Times.Once);
            _mapper.Verify(x => x.Map<DoctorDto>(doctor), Times.Once);
        }

        [Fact]
        public async Task GetDoctorById_ShouldThrowNotFoundException_WhenDoctorDoesNotExist()
        {
            int doctorId = 10;
            _doctorRepository.Setup(x => x.GetByIdAsync(doctorId)).ReturnsAsync((Doctor)null);

            Func<Task> action = async () => await _service.GetDoctorById(doctorId);

            await action.Should().ThrowAsync<NotFoundException>().WithMessage("Doctor not found.");
            _doctorRepository.Verify(x => x.GetByIdAsync(doctorId), Times.Once);
        }

        // --- GetDoctorsAsync ---
        [Fact]
        public async Task GetDoctorsAsync_ShouldReturnPagedDoctors_WhenDoctorsExist()
        {
            var request = new PaginationRequest { PageNumber = 1, PageSize = 10 };
            var doctors = new List<Doctor> { new Doctor { DoctorId = 1, FullName = "A" }, new Doctor { DoctorId = 2, FullName = "B" } };
            var paged = new PagedResult<Doctor> { Items = doctors, TotalCount = 2, PageNumber = 1, PageSize = 10 };
            var dtos = new List<DoctorDto> { new DoctorDto { DoctorId = 1 }, new DoctorDto { DoctorId = 2 } };

            _doctorRepository.Setup(r => r.GetDoctorsAsync(request, null, null, It.IsAny<CancellationToken>())).ReturnsAsync(paged);
            _mapper.Setup(m => m.Map<IEnumerable<DoctorDto>>(doctors)).Returns(dtos);

            var result = await _service.GetDoctorsAsync(request, null, null);

            result.Should().NotBeNull();
            result.TotalCount.Should().Be(2);
            result.Items.Should().HaveCount(2);
            _doctorRepository.Verify(r => r.GetDoctorsAsync(request, null, null, It.IsAny<CancellationToken>()), Times.Once);
            _mapper.Verify(m => m.Map<IEnumerable<DoctorDto>>(doctors), Times.Once);
        }

        [Fact]
        public async Task GetDoctorsAsync_ShouldPassSearchAndFilter()
        {
            var request = new PaginationRequest();
            _doctorRepository.Setup(r => r.GetDoctorsAsync(request, Specialisation.Neurology, "John", It.IsAny<CancellationToken>())).ReturnsAsync(new PagedResult<Doctor>());

            await _service.GetDoctorsAsync(request, Specialisation.Neurology, "John");

            _doctorRepository.Verify(r => r.GetDoctorsAsync(request, Specialisation.Neurology, "John", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetDoctorsAsync_ShouldPropagateRepositoryException()
        {
            var request = new PaginationRequest();
            _doctorRepository.Setup(r => r.GetDoctorsAsync(request, null, null, It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("DB"));

            Func<Task> action = async () => await _service.GetDoctorsAsync(request, null, null);
            await action.Should().ThrowAsync<Exception>().WithMessage("DB");
        }

        // --- UpdateDoctor ---
        [Fact]
        public async Task UpdateDoctor_ShouldThrowNotFound_WhenDoctorDoesNotExist()
        {
            _doctorRepository.Setup(r => r.GetByIdAsync(5)).ReturnsAsync((Doctor)null);

            Func<Task> action = async () => await _service.UpdateDoctor(5, new HealthAxis.Shared.DTOs.AdminDtos.UpdateDoctorDto());
            await action.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task UpdateDoctor_ShouldMapAndCallRepository_WhenDoctorExists()
        {
            var doctor = new Doctor { DoctorId = 3, FullName = "Old" };
            var dto = new HealthAxis.Shared.DTOs.AdminDtos.UpdateDoctorDto { FullName = "New", Specialisation = Specialisation.Cardiology };

            _doctorRepository.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(doctor);
            _doctorRepository.Setup(r => r.UpdateAsync(3, It.IsAny<Doctor>())).ReturnsAsync(doctor);
            _mapper.Setup(m => m.Map<DoctorDto>(doctor)).Returns(new DoctorDto { DoctorId = 3, FullName = "New" });

            var result = await _service.UpdateDoctor(3, dto);

            _mapper.Verify(m => m.Map(dto, doctor), Times.Once);
            _doctorRepository.Verify(r => r.UpdateAsync(3, It.IsAny<Doctor>()), Times.Once);
            result.DoctorId.Should().Be(3);
        }

        // --- GetAvailableDoctorsAsync ---
        [Fact]
        public async Task GetAvailableDoctorsAsync_ShouldReturnMappedDoctors()
        {
            var doctors = new List<Doctor> { new Doctor { DoctorId = 1 } };
            _doctorRepository.Setup(r => r.GetAvailableDoctorsAsync(null, null, It.IsAny<CancellationToken>())).ReturnsAsync(doctors);
            _mapper.Setup(m => m.Map<IEnumerable<DoctorDto>>(doctors)).Returns(new List<DoctorDto> { new DoctorDto { DoctorId = 1 } });

            var result = await _service.GetAvailableDoctorsAsync(null, null);
            result.Should().HaveCount(1);
            _doctorRepository.Verify(r => r.GetAvailableDoctorsAsync(null, null, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetAvailableDoctorsAsync_ShouldPropagateException()
        {
            _doctorRepository.Setup(r => r.GetAvailableDoctorsAsync(null, null, It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("fail"));
            Func<Task> action = async () => await _service.GetAvailableDoctorsAsync(null, null);
            await action.Should().ThrowAsync<Exception>().WithMessage("fail");
        }

        // --- Appointments retrieval ---
        [Fact]
        public async Task GetAppointmentsAsync_ShouldThrow_WhenDoctorNotFound()
        {
            _doctorRepository.Setup(r => r.GetByUserIdAsync("u", It.IsAny<CancellationToken>())).ReturnsAsync((Doctor)null);
            Func<Task> action = async () => await _service.GetAppointmentsAsync("u", CancellationToken.None);
            await action.Should().ThrowAsync<NotFoundException>().WithMessage("Doctor not found.");
        }

        [Fact]
        public async Task GetAppointmentsAsync_ShouldReturnMappedAppointments()
        {
            var doctor = new Doctor { DoctorId = 7 };
            var appts = new List<Appointment> { new Appointment { AppointmentId = 11 } };
            _doctorRepository.Setup(r => r.GetByUserIdAsync("u", It.IsAny<CancellationToken>())).ReturnsAsync(doctor);
            _doctorRepository.Setup(r => r.GetAppointmentsAsync(7, It.IsAny<CancellationToken>())).ReturnsAsync(appts);
            _mapper.Setup(m => m.Map<IEnumerable<AppointmentDto>>(appts)).Returns(new List<AppointmentDto> { new AppointmentDto { AppointmentId = 11 } });

            var result = await _service.GetAppointmentsAsync("u", CancellationToken.None);
            result.Should().HaveCount(1);
            _doctorRepository.Verify(r => r.GetAppointmentsAsync(7, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetTodaysAppointmentsAsync_ShouldThrow_WhenDoctorNotFound()
        {
            _doctorRepository.Setup(r => r.GetByUserIdAsync("u", It.IsAny<CancellationToken>())).ReturnsAsync((Doctor)null);
            Func<Task> action = async () => await _service.GetTodaysAppointmentsAsync("u", CancellationToken.None);
            await action.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task GetTodaysAppointmentsAsync_ShouldReturnMapped()
        {
            var doctor = new Doctor { DoctorId = 8 };
            var appts = new List<Appointment> { new Appointment { AppointmentId = 12, ScheduledDate = DateTime.Today } };
            _doctorRepository.Setup(r => r.GetByUserIdAsync("u", It.IsAny<CancellationToken>())).ReturnsAsync(doctor);
            _doctorRepository.Setup(r => r.GetTodaysAppointmentsAsync(8, DateTime.Today, It.IsAny<CancellationToken>())).ReturnsAsync(appts);
            _mapper.Setup(m => m.Map<IEnumerable<AppointmentDto>>(appts)).Returns(new List<AppointmentDto> { new AppointmentDto { AppointmentId = 12 } });

            var result = await _service.GetTodaysAppointmentsAsync("u", CancellationToken.None);
            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetWeeklyAppointmentsAsync_ShouldReturnMapped_AndUseWeekRange()
        {
            var doctor = new Doctor { DoctorId = 9 };
            DateTime today = DateTime.Today;
            int diff = today.DayOfWeek == DayOfWeek.Sunday ? 6 : (int)today.DayOfWeek - 1;
            DateTime start = today.AddDays(-diff);
            DateTime end = start.AddDays(6);

            var appts = new List<Appointment> { new Appointment { AppointmentId = 13 } };
            _doctorRepository.Setup(r => r.GetByUserIdAsync("u", It.IsAny<CancellationToken>())).ReturnsAsync(doctor);
            _doctorRepository.Setup(r => r.GetWeeklyAppointmentsAsync(9, start, end, It.IsAny<CancellationToken>())).ReturnsAsync(appts);
            _mapper.Setup(m => m.Map<IEnumerable<AppointmentDto>>(appts)).Returns(new List<AppointmentDto> { new AppointmentDto { AppointmentId = 13 } });

            var result = await _service.GetWeeklyAppointmentsAsync("u", CancellationToken.None);
            result.Should().HaveCount(1);
            _doctorRepository.Verify(r => r.GetWeeklyAppointmentsAsync(9, start, end, It.IsAny<CancellationToken>()), Times.Once);
        }

        // --- Delegating methods ---
        [Fact]
        public async Task UpdateAppointmentStatusAsync_ForwardsCallToAppointmentService()
        {
            var dto = new UpdateAppointmentStatusDto { Status = AppointmentStatus.Confirmed };
            var ret = new AppointmentDto { AppointmentId = 20, Status = AppointmentStatus.Confirmed };
            _appointmentService.Setup(a => a.UpdateStatusAsync(20, dto, It.IsAny<CancellationToken>())).ReturnsAsync(ret);

            var result = await _service.UpdateAppointmentStatusAsync(20, dto);
            result.Should().Be(ret);
            _appointmentService.Verify(a => a.UpdateStatusAsync(20, dto, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task AddHealthRecordAsync_ForwardsToHealthRecordService()
        {
            var dto = new HealthAxis.Shared.DTOs.HealthRecordDtos.CreateHealthRecordDto();
            var ret = new HealthAxis.Shared.DTOs.HealthRecordDtos.HealthRecordDto { RecordId = 5 };
            _healthRecordService.Setup(h => h.AddAsync(dto, It.IsAny<CancellationToken>())).ReturnsAsync(ret);

            var result = await _service.AddHealthRecordAsync(dto);
            result.Should().Be(ret);
            _healthRecordService.Verify(h => h.AddAsync(dto, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetHealthRecordByIdAsync_ForwardsToService()
        {
            var ret = new HealthAxis.Shared.DTOs.HealthRecordDtos.HealthRecordDto { RecordId = 7 };
            _healthRecordService.Setup(h => h.GetByRecordIdAsync(7, It.IsAny<CancellationToken>())).ReturnsAsync(ret);

            var result = await _service.GetHealthRecordByIdAsync(7);
            result.Should().Be(ret);
            _healthRecordService.Verify(h => h.GetByRecordIdAsync(7, It.IsAny<CancellationToken>()), Times.Once);
        }

        // --- GetDoctorByUserIdAsync & UpdateDoctorByUserIdAsync ---
        [Fact]
        public async Task GetDoctorByUserIdAsync_ShouldReturnMapped_WhenExists()
        {
            var doctor = new Doctor { DoctorId = 100 };
            var dto = new DoctorDto { DoctorId = 100 };
            _doctorRepository.Setup(r => r.GetByUserIdAsync("u", It.IsAny<CancellationToken>())).ReturnsAsync(doctor);
            _mapper.Setup(m => m.Map<DoctorDto>(doctor)).Returns(dto);

            var result = await _service.GetDoctorByUserIdAsync("u");
            result.Should().Be(dto);
        }

        [Fact]
        public async Task GetDoctorByUserIdAsync_ShouldThrow_WhenNotFound()
        {
            _doctorRepository.Setup(r => r.GetByUserIdAsync("u", It.IsAny<CancellationToken>())).ReturnsAsync((Doctor)null);
            Func<Task> action = async () => await _service.GetDoctorByUserIdAsync("u");
            await action.Should().ThrowAsync<NotFoundException>().WithMessage("Doctor profile not found.");
        }

        [Fact]
        public async Task UpdateDoctorByUserIdAsync_ShouldThrow_WhenNotFound()
        {
            _doctorRepository.Setup(r => r.GetByUserIdAsync("u", It.IsAny<CancellationToken>())).ReturnsAsync((Doctor)null);
            Func<Task> action = async () => await _service.UpdateDoctorByUserIdAsync("u", new HealthAxis.Shared.DTOs.AdminDtos.UpdateDoctorDto());
            await action.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task UpdateDoctorByUserIdAsync_ShouldMapAndUpdate_WhenExists()
        {
            var doctor = new Doctor { DoctorId = 200 };
            var dto = new HealthAxis.Shared.DTOs.AdminDtos.UpdateDoctorDto { FullName = "X", Specialisation = Specialisation.Dermatology };
            _doctorRepository.Setup(r => r.GetByUserIdAsync("u", It.IsAny<CancellationToken>())).ReturnsAsync(doctor);
            _doctorRepository.Setup(r => r.UpdateAsync(200, doctor, It.IsAny<CancellationToken>())).ReturnsAsync(doctor);
            _mapper.Setup(m => m.Map<DoctorDto>(doctor)).Returns(new DoctorDto { DoctorId = 200 });

            var result = await _service.UpdateDoctorByUserIdAsync("u", dto);
            _mapper.Verify(m => m.Map(dto, doctor), Times.Once);
            _doctorRepository.Verify(r => r.UpdateAsync(200, doctor, It.IsAny<CancellationToken>()), Times.Once);
            result.DoctorId.Should().Be(200);
        }

        // --- Dashboard ---
        [Fact]
        public async Task GetDashboardAsync_ShouldThrow_WhenNull()
        {
            _doctorRepository.Setup(r => r.GetDashboardAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((DoctorDashboardDto)null);
            Func<Task> action = async () => await _service.GetDashboardAsync(1);
            await action.Should().ThrowAsync<NotFoundException>().WithMessage("Doctor not found.");
        }

        [Fact]
        public async Task GetDashboardAsync_ShouldReturnDashboard_WhenExists()
        {
            var dash = new DoctorDashboardDto { /* properties may vary */ };
            _doctorRepository.Setup(r => r.GetDashboardAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(dash);

            var result = await _service.GetDashboardAsync(2);
            result.Should().Be(dash);
        }
    }
}
