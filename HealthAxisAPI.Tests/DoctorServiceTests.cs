using AutoMapper;
using FluentAssertions;
using HealthAxis.API.Data;
using HealthAxis.API.Exceptions;
using HealthAxis.API.Models;
using HealthAxis.API.Repositories.Interfaces;
using HealthAxis.API.Services.Implementations;
using HealthAxis.API.Services.Interfaces;
using HealthAxis.Shared.Common;
using HealthAxis.Shared.DTOs.AdminDtos;
using HealthAxis.Shared.DTOs.AppointmentDtos;
using HealthAxis.Shared.DTOs.DoctorDtos;
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
        private readonly Mock<IAppointmentRepository> _appointmentRepository;
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
            _appointmentRepository = new Mock<IAppointmentRepository>();

            _service = new DoctorService(
                _doctorRepository.Object,
                _userManager.Object,
                _context.Object,
                _mapper.Object,
                _appointmentService.Object,
                _healthRecordService.Object,
                _appointmentRepository.Object,
                _healthRecordRepository.Object);
        }

        // CreateDoctor uses _context.Database.BeginTransactionAsync() and
        // _context.Doctors.Add(...), which aren't practically mockable with
        // Mock<ApplicationDbContext>. Those tests get a real EF Core InMemory
        // context and a fresh DoctorService instance built from it.
        private (DoctorService Service, ApplicationDbContext Context) CreateServiceWithInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new ApplicationDbContext(options);

            var service = new DoctorService(
                _doctorRepository.Object,
                _userManager.Object,
                context,
                _mapper.Object,
                _appointmentService.Object,
                _healthRecordService.Object,
                _appointmentRepository.Object,
                _healthRecordRepository.Object);

            return (service, context);
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

        // --- CreateDoctor ---
        [Fact]
        public async Task CreateDoctor_ShouldThrowBusinessRule_WhenEmailAlreadyExists()
        {
            var (service, context) = CreateServiceWithInMemoryContext();

            var existing = new ApplicationUser { Id = "existing", Email = "dup@x.com" };
            _userManager.Setup(u => u.FindByEmailAsync("dup@x.com")).ReturnsAsync(existing);

            var dto = new CreateDoctorDto
            {
                Email = "dup@x.com",
                Password = "Passw0rd!",
                FullName = "Dr Dup",
                Specialisation = Specialisation.Cardiology,
                YearsOfExperience = 5,
                ConsultationFee = 100
            };

            Func<Task> action = async () => await service.CreateDoctor(dto);

            await action.Should().ThrowAsync<BusinessRuleException>().WithMessage("Email already exists.");
            context.Doctors.Should().BeEmpty();

            await context.DisposeAsync();
        }

        [Fact]
        public async Task CreateDoctor_ShouldThrowBusinessRule_AndRollback_WhenUserCreationFails()
        {
            var (service, context) = CreateServiceWithInMemoryContext();

            _userManager.Setup(u => u.FindByEmailAsync("bad@x.com")).ReturnsAsync((ApplicationUser)null);
            _userManager
                .Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Weak password" }));

            var dto = new CreateDoctorDto
            {
                Email = "bad@x.com",
                Password = "weak",
                FullName = "Dr Bad",
                Specialisation = Specialisation.Cardiology,
                YearsOfExperience = 2,
                ConsultationFee = 50
            };

            Func<Task> action = async () => await service.CreateDoctor(dto);

            await action.Should().ThrowAsync<BusinessRuleException>().WithMessage("Weak password");
            context.Doctors.Should().BeEmpty();

            await context.DisposeAsync();
        }

        [Fact]
        public async Task CreateDoctor_ShouldSucceed_WhenValid()
        {
            var (service, context) = CreateServiceWithInMemoryContext();

            _userManager.Setup(u => u.FindByEmailAsync("new@x.com")).ReturnsAsync((ApplicationUser)null);
            _userManager
                .Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success)
                .Callback<ApplicationUser, string>((u, p) => u.Id = "new-doctor-user-id");
            _userManager
                .Setup(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Doctor"))
                .ReturnsAsync(IdentityResult.Success);
            _mapper
                .Setup(m => m.Map<DoctorDto>(It.IsAny<Doctor>()))
                .Returns((Doctor d) => new DoctorDto { DoctorId = d.DoctorId, FullName = d.FullName });

            var dto = new CreateDoctorDto
            {
                Email = "new@x.com",
                Password = "Passw0rd!",
                FullName = "Dr New",
                Specialisation = Specialisation.Neurology,
                YearsOfExperience = 8,
                ConsultationFee = 200
            };

            var result = await service.CreateDoctor(dto);

            result.Should().NotBeNull();
            result.FullName.Should().Be("Dr New");

            context.Doctors.Should().ContainSingle(d => d.UserId == "new-doctor-user-id" && d.FullName == "Dr New" && d.IsActive);
            _userManager.Verify(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Doctor"), Times.Once);

            await context.DisposeAsync();
        }

        [Fact]
        public async Task CreateDoctor_ShouldRollbackAndRethrow_WhenExceptionOccursMidTransaction()
        {
            var (service, context) = CreateServiceWithInMemoryContext();

            _userManager.Setup(u => u.FindByEmailAsync("crash@x.com")).ReturnsAsync((ApplicationUser)null);
            _userManager
                .Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
            _userManager
                .Setup(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Doctor"))
                .ThrowsAsync(new Exception("role assignment failed"));

            var dto = new CreateDoctorDto
            {
                Email = "crash@x.com",
                Password = "Passw0rd!",
                FullName = "Dr Crash",
                Specialisation = Specialisation.Cardiology,
                YearsOfExperience = 3,
                ConsultationFee = 75
            };

            Func<Task> action = async () => await service.CreateDoctor(dto);

            await action.Should().ThrowAsync<Exception>().WithMessage("role assignment failed");
            context.Doctors.Should().BeEmpty();

            await context.DisposeAsync();
        }

        // --- UpdateDoctor ---
        [Fact]
        public async Task UpdateDoctor_ShouldThrowNotFound_WhenDoctorDoesNotExist()
        {
            _doctorRepository.Setup(r => r.GetByIdAsync(5)).ReturnsAsync((Doctor)null);

            Func<Task> action = async () => await _service.UpdateDoctor(5, new HealthAxis.Shared.DTOs.DoctorDtos.UpdateDoctorDto());
            await action.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task UpdateDoctor_ShouldMapAndCallRepository_WhenDoctorExists()
        {
            var doctor = new Doctor { DoctorId = 3, FullName = "Old" };
            var dto = new HealthAxis.Shared.DTOs.DoctorDtos.UpdateDoctorDto { FullName = "New", Specialisation = Specialisation.Cardiology };

            _doctorRepository.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(doctor);
            _doctorRepository.Setup(r => r.UpdateAsync(3, It.IsAny<Doctor>())).ReturnsAsync(doctor);
            _mapper.Setup(m => m.Map<DoctorDto>(doctor)).Returns(new DoctorDto { DoctorId = 3, FullName = "New" });

            var result = await _service.UpdateDoctor(3, dto);

            _mapper.Verify(m => m.Map(dto, doctor), Times.Once);
            _doctorRepository.Verify(r => r.UpdateAsync(3, It.IsAny<Doctor>()), Times.Once);
            result.DoctorId.Should().Be(3);
        }

        [Fact]
        public async Task UpdateDoctor_ShouldPropagateException_WhenRepositoryUpdateFails()
        {
            var doctor = new Doctor { DoctorId = 4, FullName = "Old" };
            _doctorRepository.Setup(r => r.GetByIdAsync(4)).ReturnsAsync(doctor);
            _doctorRepository.Setup(r => r.UpdateAsync(4, It.IsAny<Doctor>())).ThrowsAsync(new Exception("update failed"));

            Func<Task> action = async () => await _service.UpdateDoctor(4, new HealthAxis.Shared.DTOs.DoctorDtos.UpdateDoctorDto());

            await action.Should().ThrowAsync<Exception>().WithMessage("update failed");
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
        public async Task GetWeeklyAppointmentsAsync_ShouldThrow_WhenDoctorNotFound()
        {
            _doctorRepository.Setup(r => r.GetByUserIdAsync("u", It.IsAny<CancellationToken>())).ReturnsAsync((Doctor)null);
            Func<Task> action = async () => await _service.GetWeeklyAppointmentsAsync("u", CancellationToken.None);
            await action.Should().ThrowAsync<NotFoundException>().WithMessage("Doctor not found.");
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
        public async Task UpdateAppointmentStatusAsync_ShouldPropagateException()
        {
            var dto = new UpdateAppointmentStatusDto { Status = AppointmentStatus.Cancelled };
            _appointmentService.Setup(a => a.UpdateStatusAsync(21, dto, It.IsAny<CancellationToken>())).ThrowsAsync(new NotFoundException("Appointment not found."));

            Func<Task> action = async () => await _service.UpdateAppointmentStatusAsync(21, dto);

            await action.Should().ThrowAsync<NotFoundException>().WithMessage("Appointment not found.");
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
        public async Task AddHealthRecordAsync_ShouldPropagateException()
        {
            var dto = new HealthAxis.Shared.DTOs.HealthRecordDtos.CreateHealthRecordDto();
            _healthRecordService.Setup(h => h.AddAsync(dto, It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("save failed"));

            Func<Task> action = async () => await _service.AddHealthRecordAsync(dto);

            await action.Should().ThrowAsync<Exception>().WithMessage("save failed");
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

        [Fact]
        public async Task GetHealthRecordByIdAsync_ShouldPropagateException()
        {
            _healthRecordService.Setup(h => h.GetByRecordIdAsync(99, It.IsAny<CancellationToken>())).ThrowsAsync(new NotFoundException("Health record not found."));

            Func<Task> action = async () => await _service.GetHealthRecordByIdAsync(99);

            await action.Should().ThrowAsync<NotFoundException>().WithMessage("Health record not found.");
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
            Func<Task> action = async () => await _service.UpdateDoctorByUserIdAsync("u", new HealthAxis.Shared.DTOs.DoctorDtos.UpdateDoctorDto());
            await action.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task UpdateDoctorByUserIdAsync_ShouldMapAndUpdate_WhenExists()
        {
            var doctor = new Doctor { DoctorId = 200 };
            var dto = new HealthAxis.Shared.DTOs.DoctorDtos.UpdateDoctorDto { FullName = "X", Specialisation = Specialisation.Dermatology };
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

        [Fact]
        public async Task GetDashboardAsync_ShouldPropagateException()
        {
            _doctorRepository.Setup(r => r.GetDashboardAsync(3, It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("dashboard failed"));

            Func<Task> action = async () => await _service.GetDashboardAsync(3);

            await action.Should().ThrowAsync<Exception>().WithMessage("dashboard failed");
        }

        // --- GetPatientHealthHistoryAsync ---
        [Fact]
        public async Task GetPatientHealthHistoryAsync_ShouldThrowNotFound_WhenAppointmentMissing()
        {
            _appointmentRepository.Setup(r => r.GetByIdAsync(50, It.IsAny<CancellationToken>())).ReturnsAsync((Appointment)null);

            Func<Task> action = async () => await _service.GetPatientHealthHistoryAsync(1, 50);

            await action.Should().ThrowAsync<NotFoundException>().WithMessage("Appointment with ID 50 was not found.");
        }

        [Fact]
        public async Task GetPatientHealthHistoryAsync_ShouldThrowBusinessRule_WhenAppointmentBelongsToDifferentPatient()
        {
            var appointment = new Appointment { AppointmentId = 51, PatientId = 999 };
            _appointmentRepository.Setup(r => r.GetByIdAsync(51, It.IsAny<CancellationToken>())).ReturnsAsync(appointment);

            Func<Task> action = async () => await _service.GetPatientHealthHistoryAsync(1, 51);

            await action.Should().ThrowAsync<BusinessRuleException>()
                .WithMessage("The appointment does not belong to the specified patient.");
        }

        [Fact]
        public async Task GetPatientHealthHistoryAsync_ShouldReturnMapped_WhenValid()
        {
            var appointment = new Appointment { AppointmentId = 52, PatientId = 1 };
            var records = new List<HealthAxis.API.Models.HealthRecord> { new HealthAxis.API.Models.HealthRecord { RecordId = 1 } };
            var dtos = new List<HealthAxis.Shared.DTOs.HealthRecordDtos.HealthRecordDto> { new HealthAxis.Shared.DTOs.HealthRecordDtos.HealthRecordDto { RecordId = 1 } };

            _appointmentRepository.Setup(r => r.GetByIdAsync(52, It.IsAny<CancellationToken>())).ReturnsAsync(appointment);
            _healthRecordRepository.Setup(r => r.GetPatientHealthRecordsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(records);
            _mapper.Setup(m => m.Map<IEnumerable<HealthAxis.Shared.DTOs.HealthRecordDtos.HealthRecordDto>>(records)).Returns(dtos);

            var result = await _service.GetPatientHealthHistoryAsync(1, 52);

            result.Should().HaveCount(1);
            _healthRecordRepository.Verify(r => r.GetPatientHealthRecordsAsync(1, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetPatientHealthHistoryAsync_ShouldPropagateException_WhenRepositoryFails()
        {
            _appointmentRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("lookup failed"));

            Func<Task> action = async () => await _service.GetPatientHealthHistoryAsync(1, 53);

            await action.Should().ThrowAsync<Exception>().WithMessage("lookup failed");
        }
    }
}