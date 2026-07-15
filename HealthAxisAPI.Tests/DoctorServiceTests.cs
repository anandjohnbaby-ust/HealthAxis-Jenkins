using AutoMapper;
using FluentAssertions;
using HealthAxis.API.Events;
using HealthAxis.API.Exceptions;
using HealthAxis.API.Models;
using HealthAxis.API.Repositories.Interfaces;
using HealthAxis.API.Services.Implementations;
using HealthAxis.Shared.Common;
using HealthAxis.Shared.DTOs.AdminDtos;
using HealthAxis.Shared.DTOs.AppointmentDtos;
using HealthAxis.Shared.DTOs.DoctorDtos;
using HealthAxis.Shared.DTOs.HealthRecordDtos;
using HealthAxis.Shared.Enums;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;

namespace HealthAxis.Tests.Services
{
    public class DoctorServiceTests
    {
        protected readonly Mock<IDoctorRepository> _doctorRepository;
        protected readonly Mock<IUserStore<ApplicationUser>> _userStore;
        protected readonly Mock<UserManager<ApplicationUser>> _userManager;
        protected readonly Mock<IMapper> _mapper;
        protected readonly Mock<IAppointmentRepository> _appointmentRepository;
        protected readonly Mock<IHealthRecordRepository> _healthRecordRepository;
        protected readonly Mock<IPublishEndpoint> _publishEndpoint;
        protected readonly DoctorService _service;

        public DoctorServiceTests()
        {
            _doctorRepository = new Mock<IDoctorRepository>();
            _userStore = new Mock<IUserStore<ApplicationUser>>();

            _userManager = new Mock<UserManager<ApplicationUser>>(
                _userStore.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            _mapper = new Mock<IMapper>();
            _appointmentRepository = new Mock<IAppointmentRepository>();
            _healthRecordRepository = new Mock<IHealthRecordRepository>();
            _publishEndpoint = new Mock<IPublishEndpoint>();

            _service = new DoctorService(
                _doctorRepository.Object,
                _userManager.Object,
                _mapper.Object,
                _appointmentRepository.Object,
                _healthRecordRepository.Object,
                _publishEndpoint.Object);
        }

        #region GetDoctorById

        [Fact]
        public async Task GetDoctorById_ShouldReturnDoctor_WhenDoctorExists()
        {
            var doctor = new Doctor { DoctorId = 1, FullName = "Dr. Smith" };
            var doctorDto = new DoctorDto { DoctorId = 1, FullName = "Dr. Smith" };

            _doctorRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(doctor);
            _mapper.Setup(x => x.Map<DoctorDto>(doctor)).Returns(doctorDto);

            var result = await _service.GetDoctorById(1);

            result.Should().BeEquivalentTo(doctorDto);
        }

        [Fact]
        public async Task GetDoctorById_ShouldThrowNotFoundException_WhenDoctorDoesNotExist()
        {
            _doctorRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync((Doctor?)null);

            Func<Task> act = async () => await _service.GetDoctorById(1);

            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Doctor not found.");
        }

        #endregion

        #region GetDoctorsAsync

        [Fact]
        public async Task GetDoctorsAsync_ShouldReturnPagedResult_WhenCalled()
        {
            var request = new PaginationRequest { PageNumber = 1, PageSize = 10 };

            var pagedDoctors = new PagedResult<Doctor>
            {
                Items = new List<Doctor> { new() { DoctorId = 1 } },
                TotalCount = 1,
                PageNumber = 1,
                PageSize = 10
            };

            var doctorDtos = new List<DoctorDto> { new() { DoctorId = 1 } };

            _doctorRepository
                .Setup(x => x.GetDoctorsAsync(request, null, null, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(pagedDoctors);

            _mapper
                .Setup(x => x.Map<IEnumerable<DoctorDto>>(pagedDoctors.Items))
                .Returns(doctorDtos);

            var result = await _service.GetDoctorsAsync(request, null, null, null);

            result.Items.Should().BeEquivalentTo(doctorDtos);
            result.TotalCount.Should().Be(1);
        }

        #endregion

        #region CreateDoctor

        private static CreateDoctorDto ValidCreateDto() => new()
        {
            Email = "doc@example.com",
            Password = "P@ssw0rd!",
            FullName = "Dr. New",
            Specialisation = Specialisation.Cardiology,
            YearsOfExperience = 5,
            ConsultationFee = 100
        };

        [Fact]
        public async Task CreateDoctor_ShouldThrowBusinessRuleException_WhenEmailAlreadyExists()
        {
            var dto = ValidCreateDto();

            _userManager
                .Setup(x => x.FindByEmailAsync(dto.Email))
                .ReturnsAsync(new ApplicationUser { Email = dto.Email });

            Func<Task> act = async () => await _service.CreateDoctor(dto);

            await act.Should().ThrowAsync<BusinessRuleException>()
                .WithMessage("Email already exists.");
        }

        [Fact]
        public async Task CreateDoctor_ShouldThrowBusinessRuleException_WhenUserCreationFails()
        {
            var dto = ValidCreateDto();

            _userManager
                .Setup(x => x.FindByEmailAsync(dto.Email))
                .ReturnsAsync((ApplicationUser?)null);

            var identityErrors = new[]
            {
                new IdentityError { Description = "Password too weak" }
            };

            _userManager
                .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), dto.Password))
                .ReturnsAsync(IdentityResult.Failed(identityErrors));

            Func<Task> act = async () => await _service.CreateDoctor(dto);

            await act.Should().ThrowAsync<BusinessRuleException>()
                .WithMessage("Password too weak");
        }

        [Fact]
        public async Task CreateDoctor_ShouldCreateSuccessfully_WhenValidDataProvided()
        {
            var dto = ValidCreateDto();
            var doctorDto = new DoctorDto { FullName = dto.FullName };

            _userManager
                .Setup(x => x.FindByEmailAsync(dto.Email))
                .ReturnsAsync((ApplicationUser?)null);

            _userManager
                .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), dto.Password))
                .ReturnsAsync(IdentityResult.Success);

            _userManager
                .Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Doctor"))
                .ReturnsAsync(IdentityResult.Success);

            _doctorRepository
                .Setup(x => x.CreateDoctorAsync(It.IsAny<Doctor>()))
                .ReturnsAsync((Doctor d, CancellationToken _) => d); // fixed below if signature has no ct

            _mapper
                .Setup(x => x.Map<DoctorDto>(It.IsAny<Doctor>()))
                .Returns(doctorDto);

            var result = await _service.CreateDoctor(dto);

            result.Should().BeEquivalentTo(doctorDto);

            _doctorRepository.Verify(
                x => x.CreateDoctorAsync(It.Is<Doctor>(d =>
                    d.FullName == dto.FullName &&
                    d.Specialisation == dto.Specialisation &&
                    d.IsActive == true)),
                Times.Once);

            _userManager.Verify(
                x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Doctor"),
                Times.Once);
        }

        #endregion

        #region UpdateDoctor

        [Fact]
        public async Task UpdateDoctor_ShouldUpdateSuccessfully_WhenDoctorExists()
        {
            var doctor = new Doctor { DoctorId = 1, FullName = "Old Name" };
            var dto = new UpdateDoctorDto { FullName = "New Name" };
            var doctorDto = new DoctorDto { DoctorId = 1, FullName = "New Name" };

            _doctorRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(doctor);

            _mapper
                .Setup(x => x.Map(dto, doctor))
                .Returns(doctor);

            _doctorRepository
                .Setup(x => x.UpdateAsync(1, doctor))
                .ReturnsAsync(doctor);   // <-- fixed: was Task.CompletedTask

            _mapper.Setup(x => x.Map<DoctorDto>(doctor)).Returns(doctorDto);

            var result = await _service.UpdateDoctor(1, dto);

            result.Should().BeEquivalentTo(doctorDto);

            _doctorRepository.Verify(x => x.UpdateAsync(1, doctor), Times.Once);
        }

        #endregion

        #region GetAvailableDoctorsAsync

        [Fact]
        public async Task GetAvailableDoctorsAsync_ShouldReturnPagedResult_WhenCalled()
        {
            var request = new PaginationRequest { PageNumber = 1, PageSize = 10 };

            var pagedDoctors = new PagedResult<Doctor>
            {
                Items = new List<Doctor> { new() { DoctorId = 1 } },
                TotalCount = 1,
                PageNumber = 1,
                PageSize = 10
            };

            var doctorDtos = new List<DoctorDto> { new() { DoctorId = 1 } };

            _doctorRepository
                .Setup(x => x.GetAvailableDoctorsAsync(null, null, request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(pagedDoctors);

            _mapper
                .Setup(x => x.Map<List<DoctorDto>>(pagedDoctors.Items))
                .Returns(doctorDtos);

            var result = await _service.GetAvailableDoctorsAsync(null, null, request);

            result.Items.Should().BeEquivalentTo(doctorDtos);
        }

        #endregion

        #region GetAppointmentsAsync

        [Fact]
        public async Task GetAppointmentsAsync_ShouldThrowNotFoundException_WhenDoctorDoesNotExist()
        {
            _doctorRepository
                .Setup(x => x.GetByUserIdAsync("user1", It.IsAny<CancellationToken>()))
                .ReturnsAsync((Doctor?)null);

            Func<Task> act = async () =>
                await _service.GetAppointmentsAsync("user1", new PaginationRequest());

            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Doctor not found.");
        }

        [Fact]
        public async Task GetAppointmentsAsync_ShouldReturnPagedResult_WhenDoctorExists()
        {
            var doctor = new Doctor { DoctorId = 1, UserId = "user1" };
            var request = new PaginationRequest();

            var pagedAppointments = new PagedResult<Appointment>
            {
                Items = new List<Appointment> { new() { AppointmentId = 1 } },
                TotalCount = 1,
                PageNumber = 1,
                PageSize = 10
            };

            var appointmentDtos = new List<AppointmentDto> { new() { AppointmentId = 1 } };

            _doctorRepository
                .Setup(x => x.GetByUserIdAsync("user1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(doctor);

            _doctorRepository
                .Setup(x => x.GetAppointmentsAsync(1, request, null, null, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(pagedAppointments);

            _mapper
                .Setup(x => x.Map<List<AppointmentDto>>(pagedAppointments.Items))
                .Returns(appointmentDtos);

            var result = await _service.GetAppointmentsAsync("user1", request);

            result.Items.Should().BeEquivalentTo(appointmentDtos);
        }

        #endregion

        #region GetTodaysAppointmentsAsync

        [Fact]
        public async Task GetTodaysAppointmentsAsync_ShouldThrowNotFoundException_WhenDoctorDoesNotExist()
        {
            _doctorRepository
                .Setup(x => x.GetByUserIdAsync("user1", It.IsAny<CancellationToken>()))
                .ReturnsAsync((Doctor?)null);

            Func<Task> act = async () =>
                await _service.GetTodaysAppointmentsAsync("user1", new PaginationRequest());

            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Doctor not found.");
        }

        [Fact]
        public async Task GetTodaysAppointmentsAsync_ShouldReturnPagedResult_WhenDoctorExists()
        {
            var doctor = new Doctor { DoctorId = 1, UserId = "user1" };
            var request = new PaginationRequest();

            var pagedAppointments = new PagedResult<Appointment>
            {
                Items = new List<Appointment> { new() { AppointmentId = 1 } },
                TotalCount = 1,
                PageNumber = 1,
                PageSize = 10
            };

            var appointmentDtos = new List<AppointmentDto> { new() { AppointmentId = 1 } };

            _doctorRepository
                .Setup(x => x.GetByUserIdAsync("user1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(doctor);

            _doctorRepository
                .Setup(x => x.GetTodaysAppointmentsAsync(1, It.IsAny<DateTime>(), request, null, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(pagedAppointments);

            _mapper
                .Setup(x => x.Map<List<AppointmentDto>>(pagedAppointments.Items))
                .Returns(appointmentDtos);

            var result = await _service.GetTodaysAppointmentsAsync("user1", request);

            result.Items.Should().BeEquivalentTo(appointmentDtos);
        }

        #endregion

        #region GetWeeklyAppointmentsAsync

        [Fact]
        public async Task GetWeeklyAppointmentsAsync_ShouldThrowNotFoundException_WhenDoctorDoesNotExist()
        {
            _doctorRepository
                .Setup(x => x.GetByUserIdAsync("user1", It.IsAny<CancellationToken>()))
                .ReturnsAsync((Doctor?)null);

            Func<Task> act = async () =>
                await _service.GetWeeklyAppointmentsAsync("user1", new PaginationRequest());

            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Doctor not found.");
        }

        [Fact]
        public async Task GetWeeklyAppointmentsAsync_ShouldReturnPagedResult_WhenDoctorExists()
        {
            var doctor = new Doctor { DoctorId = 1, UserId = "user1" };
            var request = new PaginationRequest();

            var pagedAppointments = new PagedResult<Appointment>
            {
                Items = new List<Appointment> { new() { AppointmentId = 1 } },
                TotalCount = 1,
                PageNumber = 1,
                PageSize = 10
            };

            var appointmentDtos = new List<AppointmentDto> { new() { AppointmentId = 1 } };

            _doctorRepository
                .Setup(x => x.GetByUserIdAsync("user1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(doctor);

            _doctorRepository
                .Setup(x => x.GetWeeklyAppointmentsAsync(1, request, null, null, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(pagedAppointments);

            _mapper
                .Setup(x => x.Map<List<AppointmentDto>>(pagedAppointments.Items))
                .Returns(appointmentDtos);

            var result = await _service.GetWeeklyAppointmentsAsync("user1", request);

            result.Items.Should().BeEquivalentTo(appointmentDtos);
        }

        #endregion

        #region GetDoctorByUserIdAsync

        [Fact]
        public async Task GetDoctorByUserIdAsync_ShouldThrowNotFoundException_WhenDoctorProfileDoesNotExist()
        {
            _doctorRepository
                .Setup(x => x.GetByUserIdAsync("user1", It.IsAny<CancellationToken>()))
                .ReturnsAsync((Doctor?)null);

            Func<Task> act = async () => await _service.GetDoctorByUserIdAsync("user1");

            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Doctor profile not found.");
        }

        [Fact]
        public async Task GetDoctorByUserIdAsync_ShouldReturnDoctor_WhenProfileExists()
        {
            var doctor = new Doctor { DoctorId = 1, UserId = "user1" };
            var doctorDto = new DoctorDto { DoctorId = 1 };

            _doctorRepository
                .Setup(x => x.GetByUserIdAsync("user1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(doctor);

            _mapper.Setup(x => x.Map<DoctorDto>(doctor)).Returns(doctorDto);

            var result = await _service.GetDoctorByUserIdAsync("user1");

            result.Should().BeEquivalentTo(doctorDto);
        }

        #endregion

        #region UpdateDoctorByUserIdAsync

        [Fact]
        public async Task UpdateDoctorByUserIdAsync_ShouldThrowNotFoundException_WhenProfileDoesNotExist()
        {
            _doctorRepository
                .Setup(x => x.GetByUserIdAsync("user1", It.IsAny<CancellationToken>()))
                .ReturnsAsync((Doctor?)null);

            Func<Task> act = async () =>
                await _service.UpdateDoctorByUserIdAsync("user1", new UpdateDoctorDto());

            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Doctor profile not found.");
        }

        [Fact]
        public async Task UpdateDoctorByUserIdAsync_ShouldUpdateSuccessfully_WhenProfileExists()
        {
            var doctor = new Doctor { DoctorId = 1, UserId = "user1", FullName = "Old" };
            var dto = new UpdateDoctorDto { FullName = "New" };
            var doctorDto = new DoctorDto { DoctorId = 1, FullName = "New" };

            _doctorRepository
                .Setup(x => x.GetByUserIdAsync("user1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(doctor);

            _mapper.Setup(x => x.Map(dto, doctor)).Returns(doctor);

            _doctorRepository
                .Setup(x => x.UpdateAsync(1, doctor, It.IsAny<CancellationToken>()))
                .ReturnsAsync(doctor);   // <-- fixed: was Task.CompletedTask

            _mapper.Setup(x => x.Map<DoctorDto>(doctor)).Returns(doctorDto);

            var result = await _service.UpdateDoctorByUserIdAsync("user1", dto);

            result.Should().BeEquivalentTo(doctorDto);
        }

        #endregion

        #region GetDashboardAsync

        [Fact]
        public async Task GetDashboardAsync_ShouldThrowNotFoundException_WhenDashboardIsNull()
        {
            _doctorRepository
                .Setup(x => x.GetDashboardAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync((DoctorDashboardDto?)null);

            Func<Task> act = async () => await _service.GetDashboardAsync(1);

            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Doctor not found.");
        }

        [Fact]
        public async Task GetDashboardAsync_ShouldReturnDashboard_WhenFound()
        {
            var dashboard = new DoctorDashboardDto();

            _doctorRepository
                .Setup(x => x.GetDashboardAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(dashboard);

            var result = await _service.GetDashboardAsync(1);

            result.Should().BeSameAs(dashboard);
        }

        #endregion

        #region GetPatientHealthHistoryAsync

        [Fact]
        public async Task GetPatientHealthHistoryAsync_ShouldThrowNotFoundException_WhenAppointmentDoesNotExist()
        {
            _appointmentRepository
                .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Appointment?)null);

            Func<Task> act = async () =>
                await _service.GetPatientHealthHistoryAsync(5, 1);

            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Appointment with ID 1 was not found.");
        }

        [Fact]
        public async Task GetPatientHealthHistoryAsync_ShouldThrowBusinessRuleException_WhenAppointmentBelongsToDifferentPatient()
        {
            _appointmentRepository
                .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Appointment { AppointmentId = 1, PatientId = 999 });

            Func<Task> act = async () =>
                await _service.GetPatientHealthHistoryAsync(5, 1);

            await act.Should().ThrowAsync<BusinessRuleException>()
                .WithMessage("The appointment does not belong to the specified patient.");
        }

        [Fact]
        public async Task GetPatientHealthHistoryAsync_ShouldReturnRecords_WhenAppointmentBelongsToPatient()
        {
            var appointment = new Appointment { AppointmentId = 1, PatientId = 5 };
            var records = new List<HealthRecord> { new() { RecordId = 1, PatientId = 5 } };
            var recordDtos = new List<HealthRecordDto> { new() { RecordId = 1 } };

            _appointmentRepository
                .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointment);

            _healthRecordRepository
                .Setup(x => x.GetPatientHealthRecordsAsync(5, It.IsAny<CancellationToken>()))
                .ReturnsAsync(records);

            _mapper
                .Setup(x => x.Map<IEnumerable<HealthRecordDto>>(records))
                .Returns(recordDtos);

            var result = await _service.GetPatientHealthHistoryAsync(5, 1);

            result.Should().BeEquivalentTo(recordDtos);
        }

        #endregion

        #region UpdateStatusAsync

        [Fact]
        public async Task UpdateStatusAsync_ShouldThrowNotFoundException_WhenAppointmentDoesNotExist()
        {
            _appointmentRepository
                .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Appointment?)null);

            Func<Task> act = async () =>
                await _service.UpdateStatusAsync(1, new UpdateAppointmentStatusDto { Status = AppointmentStatus.Confirmed });

            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Appointment not found.");
        }

        [Fact]
        public async Task UpdateStatusAsync_ShouldThrowValidationException_WhenAppointmentIsCancelled()
        {
            _appointmentRepository
                .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Appointment { AppointmentId = 1, Status = AppointmentStatus.Cancelled });

            Func<Task> act = async () =>
                await _service.UpdateStatusAsync(1, new UpdateAppointmentStatusDto { Status = AppointmentStatus.Confirmed });

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("Cancelled appointments cannot be modified.");
        }

        [Fact]
        public async Task UpdateStatusAsync_ShouldThrowValidationException_WhenAppointmentIsCompleted()
        {
            _appointmentRepository
                .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Appointment { AppointmentId = 1, Status = AppointmentStatus.Completed });

            Func<Task> act = async () =>
                await _service.UpdateStatusAsync(1, new UpdateAppointmentStatusDto { Status = AppointmentStatus.Confirmed });

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("Completed appointments cannot be modified.");
        }

        [Fact]
        public async Task UpdateStatusAsync_ShouldThrowValidationException_WhenStatusIsUnchanged()
        {
            _appointmentRepository
                .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Appointment { AppointmentId = 1, Status = AppointmentStatus.Confirmed });

            Func<Task> act = async () =>
                await _service.UpdateStatusAsync(1, new UpdateAppointmentStatusDto { Status = AppointmentStatus.Confirmed });

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("Appointment is already Confirmed.");
        }

        [Fact]
        public async Task UpdateStatusAsync_ShouldThrowValidationException_WhenCompletingPendingAppointmentDirectly()
        {
            _appointmentRepository
                .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Appointment { AppointmentId = 1, Status = AppointmentStatus.Pending });

            Func<Task> act = async () =>
                await _service.UpdateStatusAsync(1, new UpdateAppointmentStatusDto { Status = AppointmentStatus.Completed });

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("Pending appointments must be confirmed before completion.");
        }

        [Fact]
        public async Task UpdateStatusAsync_ShouldThrowValidationException_WhenRevertingToPending()
        {
            _appointmentRepository
                .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Appointment { AppointmentId = 1, Status = AppointmentStatus.Confirmed });

            Func<Task> act = async () =>
                await _service.UpdateStatusAsync(1, new UpdateAppointmentStatusDto { Status = AppointmentStatus.Pending });

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("Appointments cannot be reverted to pending status.");
        }

        [Fact]
        public async Task UpdateStatusAsync_ShouldThrowNotFoundException_WhenUpdateReturnsNull()
        {
            _appointmentRepository
                .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Appointment { AppointmentId = 1, Status = AppointmentStatus.Pending });

            _appointmentRepository
                .Setup(x => x.UpdateAsync(1, It.IsAny<Appointment>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Appointment?)null);

            Func<Task> act = async () =>
                await _service.UpdateStatusAsync(1, new UpdateAppointmentStatusDto { Status = AppointmentStatus.Confirmed });

            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Appointment not found.");
        }

        [Fact]
        public async Task UpdateStatusAsync_ShouldConfirmSuccessfully_WhenPendingToConfirmed()
        {
            var appointment = new Appointment { AppointmentId = 1, Status = AppointmentStatus.Pending };
            var updated = new Appointment { AppointmentId = 1, Status = AppointmentStatus.Confirmed };
            var dto = new UpdateAppointmentStatusDto { Status = AppointmentStatus.Confirmed };
            var appointmentDto = new AppointmentDto { AppointmentId = 1 };

            _appointmentRepository
                .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointment);

            _appointmentRepository
                .Setup(x => x.UpdateAsync(1, appointment, It.IsAny<CancellationToken>()))
                .ReturnsAsync(updated);

            _publishEndpoint
                .Setup(x => x.Publish(It.IsAny<AppointmentEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mapper.Setup(x => x.Map<AppointmentDto>(updated)).Returns(appointmentDto);

            var result = await _service.UpdateStatusAsync(1, dto);

            result.Should().BeEquivalentTo(appointmentDto);

            _publishEndpoint.Verify(
                x => x.Publish(It.IsAny<AppointmentEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateStatusAsync_ShouldCancelSuccessfully_WhenConfirmedToCancelled()
        {
            var appointment = new Appointment { AppointmentId = 1, Status = AppointmentStatus.Confirmed };
            var updated = new Appointment { AppointmentId = 1, Status = AppointmentStatus.Cancelled };
            var dto = new UpdateAppointmentStatusDto
            {
                Status = AppointmentStatus.Cancelled,
                CancellationReason = "Doctor unavailable"
            };
            var appointmentDto = new AppointmentDto { AppointmentId = 1 };

            _appointmentRepository
                .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointment);

            _appointmentRepository
                .Setup(x => x.UpdateAsync(1, appointment, It.IsAny<CancellationToken>()))
                .ReturnsAsync(updated);

            _publishEndpoint
                .Setup(x => x.Publish(It.IsAny<AppointmentEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mapper.Setup(x => x.Map<AppointmentDto>(updated)).Returns(appointmentDto);

            var result = await _service.UpdateStatusAsync(1, dto);

            result.Should().BeEquivalentTo(appointmentDto);
        }

        [Fact]
        public async Task UpdateStatusAsync_ShouldCompleteSuccessfully_WhenConfirmedToCompleted()
        {
            var appointment = new Appointment { AppointmentId = 1, Status = AppointmentStatus.Confirmed };
            var updated = new Appointment { AppointmentId = 1, Status = AppointmentStatus.Completed };
            var dto = new UpdateAppointmentStatusDto { Status = AppointmentStatus.Completed };
            var appointmentDto = new AppointmentDto { AppointmentId = 1 };

            _appointmentRepository
                .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointment);

            _appointmentRepository
                .Setup(x => x.UpdateAsync(1, appointment, It.IsAny<CancellationToken>()))
                .ReturnsAsync(updated);

            _publishEndpoint
                .Setup(x => x.Publish(It.IsAny<AppointmentEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mapper.Setup(x => x.Map<AppointmentDto>(updated)).Returns(appointmentDto);

            var result = await _service.UpdateStatusAsync(1, dto);

            result.Should().BeEquivalentTo(appointmentDto);
        }

        #endregion

        #region AddAsync (Health Record)

        private static CreateHealthRecordDto ValidHealthRecordDto() => new()
        {
            AppointmentId = 1,
            Diagnosis = "Flu",
            Prescription = "Rest and fluids",
            Notes = "Follow up in a week"
        };

        [Fact]
        public async Task AddAsync_ShouldThrowNotFoundException_WhenAppointmentDoesNotExist()
        {
            var dto = ValidHealthRecordDto();

            _appointmentRepository
                .Setup(x => x.GetByIdAsync(dto.AppointmentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Appointment?)null);

            Func<Task> act = async () => await _service.AddAsync(dto);

            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Appointment not found.");
        }

        [Fact]
        public async Task AddAsync_ShouldThrowValidationException_WhenAppointmentIsNotCompleted()
        {
            var dto = ValidHealthRecordDto();

            _appointmentRepository
                .Setup(x => x.GetByIdAsync(dto.AppointmentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Appointment { AppointmentId = 1, Status = AppointmentStatus.Confirmed });

            Func<Task> act = async () => await _service.AddAsync(dto);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("Health records can only be created for completed appointments.");
        }

        [Fact]
        public async Task AddAsync_ShouldThrowValidationException_WhenHealthRecordAlreadyExists()
        {
            var dto = ValidHealthRecordDto();

            _appointmentRepository
                .Setup(x => x.GetByIdAsync(dto.AppointmentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Appointment { AppointmentId = 1, Status = AppointmentStatus.Completed });

            _healthRecordRepository
                .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<HealthRecord> { new() { AppointmentId = 1 } });

            Func<Task> act = async () => await _service.AddAsync(dto);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("Health record already exists for this appointment.");
        }

        [Fact]
        public async Task AddAsync_ShouldCreateSuccessfully_WhenAppointmentIsCompletedAndNoExistingRecord()
        {
            var dto = ValidHealthRecordDto();

            var appointment = new Appointment
            {
                AppointmentId = 1,
                DoctorId = 10,
                PatientId = 20,
                ScheduledDate = DateTime.Today,
                Status = AppointmentStatus.Completed
            };

            var savedRecord = new HealthRecord { RecordId = 99, AppointmentId = 1 };
            var recordDto = new HealthRecordDto { RecordId = 99 };

            _appointmentRepository
                .Setup(x => x.GetByIdAsync(dto.AppointmentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointment);

            _healthRecordRepository
                .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<HealthRecord>());

            _healthRecordRepository
                .Setup(x => x.AddAsync(It.IsAny<HealthRecord>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(savedRecord);

            _mapper
                .Setup(x => x.Map<HealthRecordDto>(savedRecord))
                .Returns(recordDto);

            var result = await _service.AddAsync(dto);

            result.Should().BeEquivalentTo(recordDto);

            _healthRecordRepository.Verify(
                x => x.AddAsync(
                    It.Is<HealthRecord>(hr =>
                        hr.AppointmentId == dto.AppointmentId &&
                        hr.DoctorId == appointment.DoctorId &&
                        hr.PatientId == appointment.PatientId &&
                        hr.Diagnosis == dto.Diagnosis &&
                        hr.Prescription == dto.Prescription &&
                        hr.Notes == dto.Notes),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        #endregion
    }
}