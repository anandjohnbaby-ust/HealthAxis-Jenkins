// =====================================================================================
// AppointmentServiceTests.cs
// -------------------------------------------------------------------------------------
// NOTE ON ASSUMPTIONS:
// I don't have the source for Appointment, Doctor, Patient, the DTOs, or IRepository<T>,
// only what's inferable from AppointmentService.cs and IAppointmentRepository.cs.
// The tests below assume the following shapes, which are the most natural fit for the
// code shown. If any property/method name is slightly different in your actual classes,
// rename accordingly (the test *structure*, mock setups, and assertions will still hold):
//
//   IRepository<T>            : GetByIdAsync(int, CancellationToken), AddAsync(T, CancellationToken),
//                                UpdateAsync(int, T, CancellationToken), DeleteAsync(int, CancellationToken),
//                                GetAllAsync(CancellationToken)
//   IPatientRepository         : GetByIdAsync(int, CancellationToken)   (plus IRepository<Patient> members)
//   IAppointmentRepository     : IRepository<Appointment> + GetAppointmentReportAsync(PaginationRequest)
//   Appointment                : Id, PatientId, DoctorId, ScheduledDate, Status, CancellationReason,
//                                Confirm(), Cancel(string reason), Complete()
//   Doctor                     : Id, IsActive
//   Patient                    : Id
//   CreateAppointmentDto       : PatientId, DoctorId, ScheduledDate
//   UpdateAppointmentStatusDto : Status, CancellationReason
//   CancelAppointmentDto       : CancellationReason
//
// AppointmentDto (confirmed from source):
//   AppointmentId, PatientId, PatientName, DoctorId, DoctorName, ScheduledDate, TimeSlot,
//   Status, CancellationReason, HealthRecordId
//
// Appointment/Doctor/Patient are still assumed shapes (not provided), so `appointment.Id`
// below refers to the domain entity's own Id property, which is distinct from the DTO's
// AppointmentId. Since these are concrete domain classes (not interfaces), they are
// instantiated directly rather than mocked, so Confirm()/Cancel()/Complete() run their real
// logic and we can assert on the resulting state.
// =====================================================================================

using AutoMapper;
using HealthAxis.API.Exceptions;
using HealthAxis.API.Models;
using HealthAxis.API.Repositories.Interfaces;
using HealthAxis.API.Services.Implementations;
using HealthAxis.Shared.Common;
using HealthAxis.Shared.DTOs.AdminDtos;
using HealthAxis.Shared.DTOs.AppointmentDtos;
using HealthAxis.Shared.Enums;
using Moq;
using Xunit;

namespace HealthAxis.API.Tests.Services
{
    public class AppointmentServiceTests
    {
        private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock;
        private readonly Mock<IRepository<Doctor>> _doctorRepositoryMock;
        private readonly Mock<IPatientRepository> _patientRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly AppointmentService _sut; // system under test

        public AppointmentServiceTests()
        {
            _appointmentRepositoryMock = new Mock<IAppointmentRepository>();
            _doctorRepositoryMock = new Mock<IRepository<Doctor>>();
            _patientRepositoryMock = new Mock<IPatientRepository>();
            _mapperMock = new Mock<IMapper>();

            _sut = new AppointmentService(
                _appointmentRepositoryMock.Object,
                _doctorRepositoryMock.Object,
                _patientRepositoryMock.Object,
                _mapperMock.Object);
        }

        // -------------------------------------------------------------------
        // Helpers
        // -------------------------------------------------------------------

        private static Patient CreatePatient(int id = 1) => new Patient { PatientId = id };

        private static Doctor CreateDoctor(int id = 1, bool isActive = true) =>
            new Doctor { DoctorId = id, IsActive = isActive };

        private static Appointment CreateAppointment(
            int id = 1,
            int patientId = 1,
            int doctorId = 1,
            AppointmentStatus status = AppointmentStatus.Pending,
            DateTime? scheduledDate = null) =>
            new Appointment
            {
                AppointmentId = id,
                PatientId = patientId,
                DoctorId = doctorId,
                Status = status,
                ScheduledDate = scheduledDate ?? DateTime.Today.AddDays(1)
            };

        // =====================================================================
        // GetAllAsync
        // =====================================================================

        [Fact]
        public async Task GetAllAsync_ReturnsMappedAppointments_WhenAppointmentsExist()
        {
            // Arrange
            var appointments = new List<Appointment> { CreateAppointment(1), CreateAppointment(2) };
            var expectedDtos = new List<AppointmentDto>
            {
                new AppointmentDto { AppointmentId = 1 },
                new AppointmentDto { AppointmentId = 2 }
            };

            _appointmentRepositoryMock
                .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointments);

            _mapperMock
                .Setup(m => m.Map<IEnumerable<AppointmentDto>>(appointments))
                .Returns(expectedDtos);

            // Act
            var result = await _sut.GetAllAsync();

            // Assert
            Assert.Equal(expectedDtos, result);
            _appointmentRepositoryMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsEmptyCollection_WhenNoAppointmentsExist()
        {
            // Arrange
            _appointmentRepositoryMock
                .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Appointment>());

            _mapperMock
                .Setup(m => m.Map<IEnumerable<AppointmentDto>>(It.IsAny<IEnumerable<Appointment>>()))
                .Returns(new List<AppointmentDto>());

            // Act
            var result = await _sut.GetAllAsync();

            // Assert
            Assert.Empty(result);
        }

        // =====================================================================
        // GetAppointmentReportAsync
        // =====================================================================

        [Fact]
        public async Task GetAppointmentReportAsync_ReturnsPagedResult_FromRepository()
        {
            // Arrange
            var request = new PaginationRequest { PageNumber = 1, PageSize = 10 };
            var expected = new PagedResult<AppointmentReportDto>
            {
                Items = new List<AppointmentReportDto> { new AppointmentReportDto() },
                TotalCount = 1
            };

            _appointmentRepositoryMock
                .Setup(r => r.GetAppointmentReportAsync(request))
                .ReturnsAsync(expected);

            // Act
            var result = await _sut.GetAppointmentReportAsync(request);

            // Assert
            Assert.Same(expected, result);
            _appointmentRepositoryMock.Verify(r => r.GetAppointmentReportAsync(request), Times.Once);
        }

        // =====================================================================
        // BookAppointmentAsync
        // =====================================================================

        [Fact]
        public async Task BookAppointmentAsync_ThrowsValidationException_WhenDateIsInThePast()
        {
            // Arrange
            var dto = new CreateAppointmentDto
            {
                PatientId = 1,
                DoctorId = 1,
                ScheduledDate = DateTime.Today.AddDays(-1)
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ValidationException>(() => _sut.BookAppointmentAsync(dto));
            Assert.Equal("Appointments cannot be booked for past dates.", ex.Message);

            _patientRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task BookAppointmentAsync_ThrowsValidationException_WhenDateIsMoreThanSixMonthsAhead()
        {
            // Arrange
            var dto = new CreateAppointmentDto
            {
                PatientId = 1,
                DoctorId = 1,
                ScheduledDate = DateTime.Today.AddMonths(6).AddDays(1)
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ValidationException>(() => _sut.BookAppointmentAsync(dto));
            Assert.Equal("Appointments can only be booked up to 6 months in advance.", ex.Message);
        }

        [Fact]
        public async Task BookAppointmentAsync_AllowsBooking_ExactlyAtSixMonthBoundary()
        {
            // Arrange
            var dto = new CreateAppointmentDto
            {
                PatientId = 1,
                DoctorId = 1,
                ScheduledDate = DateTime.Today.AddMonths(6)
            };

            SetupHappyPathBooking(dto);

            // Act
            var result = await _sut.BookAppointmentAsync(dto);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task BookAppointmentAsync_AllowsBooking_WhenDateIsToday()
        {
            // Arrange
            var dto = new CreateAppointmentDto
            {
                PatientId = 1,
                DoctorId = 1,
                ScheduledDate = DateTime.Today
            };

            SetupHappyPathBooking(dto);

            // Act
            var result = await _sut.BookAppointmentAsync(dto);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task BookAppointmentAsync_ThrowsNotFoundException_WhenPatientDoesNotExist()
        {
            // Arrange
            var dto = new CreateAppointmentDto
            {
                PatientId = 99,
                DoctorId = 1,
                ScheduledDate = DateTime.Today.AddDays(1)
            };

            _patientRepositoryMock
                .Setup(r => r.GetByIdAsync(dto.PatientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Patient?)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _sut.BookAppointmentAsync(dto));
            Assert.Equal("Patient not found.", ex.Message);

            _doctorRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task BookAppointmentAsync_ThrowsNotFoundException_WhenDoctorDoesNotExist()
        {
            // Arrange
            var dto = new CreateAppointmentDto
            {
                PatientId = 1,
                DoctorId = 99,
                ScheduledDate = DateTime.Today.AddDays(1)
            };

            _patientRepositoryMock
                .Setup(r => r.GetByIdAsync(dto.PatientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreatePatient(dto.PatientId));

            _doctorRepositoryMock
                .Setup(r => r.GetByIdAsync(dto.DoctorId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Doctor?)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _sut.BookAppointmentAsync(dto));
            Assert.Equal("Doctor not found.", ex.Message);
        }

        [Fact]
        public async Task BookAppointmentAsync_ThrowsValidationException_WhenDoctorIsInactive()
        {
            // Arrange
            var dto = new CreateAppointmentDto
            {
                PatientId = 1,
                DoctorId = 1,
                ScheduledDate = DateTime.Today.AddDays(1)
            };

            _patientRepositoryMock
                .Setup(r => r.GetByIdAsync(dto.PatientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreatePatient(dto.PatientId));

            _doctorRepositoryMock
                .Setup(r => r.GetByIdAsync(dto.DoctorId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateDoctor(dto.DoctorId, isActive: false));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ValidationException>(() => _sut.BookAppointmentAsync(dto));
            Assert.Equal("Appointments cannot be booked with inactive doctors.", ex.Message);

            _appointmentRepositoryMock.Verify(
                r => r.AddAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task BookAppointmentAsync_CreatesAppointmentWithPendingStatus_OnSuccess()
        {
            // Arrange
            var dto = new CreateAppointmentDto
            {
                PatientId = 1,
                DoctorId = 1,
                ScheduledDate = DateTime.Today.AddDays(1)
            };

            var mappedAppointment = CreateAppointment(patientId: dto.PatientId, doctorId: dto.DoctorId);
            var savedAppointment = CreateAppointment(id: 42, patientId: dto.PatientId, doctorId: dto.DoctorId);
            var expectedDto = new AppointmentDto { AppointmentId = 42, Status = AppointmentStatus.Pending };

            _patientRepositoryMock
                .Setup(r => r.GetByIdAsync(dto.PatientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreatePatient(dto.PatientId));

            _doctorRepositoryMock
                .Setup(r => r.GetByIdAsync(dto.DoctorId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateDoctor(dto.DoctorId, isActive: true));

            _mapperMock
                .Setup(m => m.Map<Appointment>(dto))
                .Returns(mappedAppointment);

            _appointmentRepositoryMock
                .Setup(r => r.AddAsync(
                    It.Is<Appointment>(a => a.Status == AppointmentStatus.Pending),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(savedAppointment);

            _mapperMock
                .Setup(m => m.Map<AppointmentDto>(savedAppointment))
                .Returns(expectedDto);

            // Act
            var result = await _sut.BookAppointmentAsync(dto);

            // Assert
            Assert.Equal(expectedDto, result);
            Assert.Equal(AppointmentStatus.Pending, mappedAppointment.Status);

            _appointmentRepositoryMock.Verify(
                r => r.AddAsync(
                    It.Is<Appointment>(a => a.Status == AppointmentStatus.Pending),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        private void SetupHappyPathBooking(CreateAppointmentDto dto)
        {
            var mappedAppointment = CreateAppointment(patientId: dto.PatientId, doctorId: dto.DoctorId);
            var savedAppointment = CreateAppointment(id: 1, patientId: dto.PatientId, doctorId: dto.DoctorId);

            _patientRepositoryMock
                .Setup(r => r.GetByIdAsync(dto.PatientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreatePatient(dto.PatientId));

            _doctorRepositoryMock
                .Setup(r => r.GetByIdAsync(dto.DoctorId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateDoctor(dto.DoctorId, isActive: true));

            _mapperMock
                .Setup(m => m.Map<Appointment>(dto))
                .Returns(mappedAppointment);

            _appointmentRepositoryMock
                .Setup(r => r.AddAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(savedAppointment);

            _mapperMock
                .Setup(m => m.Map<AppointmentDto>(savedAppointment))
                .Returns(new AppointmentDto { AppointmentId = savedAppointment.AppointmentId });
        }

        // =====================================================================
        // UpdateStatusAsync
        // =====================================================================

        [Fact]
        public async Task UpdateStatusAsync_ThrowsNotFoundException_WhenAppointmentDoesNotExist()
        {
            // Arrange
            _appointmentRepositoryMock
                .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Appointment?)null);

            var dto = new UpdateAppointmentStatusDto { Status = AppointmentStatus.Confirmed };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _sut.UpdateStatusAsync(1, dto));
            Assert.Equal("Appointment not found.", ex.Message);
        }

        [Fact]
        public async Task UpdateStatusAsync_ThrowsValidationException_WhenAppointmentIsAlreadyCancelled()
        {
            // Arrange
            var appointment = CreateAppointment(status: AppointmentStatus.Cancelled);
            _appointmentRepositoryMock
                .Setup(r => r.GetByIdAsync(appointment.AppointmentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointment);

            var dto = new UpdateAppointmentStatusDto { Status = AppointmentStatus.Confirmed };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ValidationException>(() => _sut.UpdateStatusAsync(appointment.AppointmentId, dto));
            Assert.Equal("Cancelled appointments cannot be modified.", ex.Message);
        }

        [Fact]
        public async Task UpdateStatusAsync_ThrowsValidationException_WhenAppointmentIsAlreadyCompleted()
        {
            // Arrange
            var appointment = CreateAppointment(status: AppointmentStatus.Completed);
            _appointmentRepositoryMock
                .Setup(r => r.GetByIdAsync(appointment.AppointmentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointment);

            var dto = new UpdateAppointmentStatusDto { Status = AppointmentStatus.Cancelled };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ValidationException>(() => _sut.UpdateStatusAsync(appointment.AppointmentId, dto));
            Assert.Equal("Completed appointments cannot be modified.", ex.Message);
        }

        [Fact]
        public async Task UpdateStatusAsync_ThrowsValidationException_WhenUpdatingToSameStatus()
        {
            // Arrange
            var appointment = CreateAppointment(status: AppointmentStatus.Confirmed);
            _appointmentRepositoryMock
                .Setup(r => r.GetByIdAsync(appointment.AppointmentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointment);

            var dto = new UpdateAppointmentStatusDto { Status = AppointmentStatus.Confirmed };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ValidationException>(() => _sut.UpdateStatusAsync(appointment.AppointmentId, dto));
            Assert.Equal("Appointment is already Confirmed.", ex.Message);
        }

        [Fact]
        public async Task UpdateStatusAsync_ThrowsValidationException_WhenCompletingPendingAppointmentDirectly()
        {
            // Arrange
            var appointment = CreateAppointment(status: AppointmentStatus.Pending);
            _appointmentRepositoryMock
                .Setup(r => r.GetByIdAsync(appointment.AppointmentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointment);

            var dto = new UpdateAppointmentStatusDto { Status = AppointmentStatus.Completed };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ValidationException>(() => _sut.UpdateStatusAsync(appointment.AppointmentId, dto));
            Assert.Equal("Pending appointments must be confirmed before completion.", ex.Message);
        }

        [Fact]
        public async Task UpdateStatusAsync_ThrowsValidationException_WhenRevertingToPending()
        {
            // Arrange
            var appointment = CreateAppointment(status: AppointmentStatus.Confirmed);
            _appointmentRepositoryMock
                .Setup(r => r.GetByIdAsync(appointment.AppointmentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointment);

            var dto = new UpdateAppointmentStatusDto { Status = AppointmentStatus.Pending };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ValidationException>(() => _sut.UpdateStatusAsync(appointment.AppointmentId, dto));
            Assert.Equal("Appointments cannot be reverted to pending status.", ex.Message);
        }

        [Fact]
        public async Task UpdateStatusAsync_ConfirmsAppointment_WhenTransitioningFromPendingToConfirmed()
        {
            // Arrange
            var appointment = CreateAppointment(status: AppointmentStatus.Pending);
            _appointmentRepositoryMock
                .Setup(r => r.GetByIdAsync(appointment.AppointmentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointment);

            _appointmentRepositoryMock
                .Setup(r => r.UpdateAsync(appointment.AppointmentId, It.IsAny<Appointment>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((int id, Appointment a, CancellationToken _) => a);

            var expectedDto = new AppointmentDto { AppointmentId = appointment.AppointmentId, Status = AppointmentStatus.Confirmed };
            _mapperMock
                .Setup(m => m.Map<AppointmentDto>(It.Is<Appointment>(a => a.Status == AppointmentStatus.Confirmed)))
                .Returns(expectedDto);

            var dto = new UpdateAppointmentStatusDto { Status = AppointmentStatus.Confirmed };

            // Act
            var result = await _sut.UpdateStatusAsync(appointment.AppointmentId, dto);

            // Assert
            Assert.Equal(AppointmentStatus.Confirmed, appointment.Status);
            Assert.Equal(expectedDto, result);
            _appointmentRepositoryMock.Verify(
                r => r.UpdateAsync(appointment.AppointmentId, It.IsAny<Appointment>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateStatusAsync_CancelsAppointment_AndSetsCancellationReason()
        {
            // Arrange
            var appointment = CreateAppointment(status: AppointmentStatus.Pending);
            _appointmentRepositoryMock
                .Setup(r => r.GetByIdAsync(appointment.AppointmentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointment);

            _appointmentRepositoryMock
                .Setup(r => r.UpdateAsync(appointment.AppointmentId, It.IsAny<Appointment>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((int id, Appointment a, CancellationToken _) => a);

            _mapperMock
                .Setup(m => m.Map<AppointmentDto>(It.IsAny<Appointment>()))
                .Returns(new AppointmentDto { AppointmentId = appointment.AppointmentId, Status = AppointmentStatus.Cancelled });

            var dto = new UpdateAppointmentStatusDto
            {
                Status = AppointmentStatus.Cancelled,
                CancellationReason = "Doctor unavailable"
            };

            // Act
            await _sut.UpdateStatusAsync(appointment.AppointmentId, dto);

            // Assert
            Assert.Equal(AppointmentStatus.Cancelled, appointment.Status);
            Assert.Equal("Doctor unavailable", appointment.CancellationReason);
        }

        [Fact]
        public async Task UpdateStatusAsync_CancelsAppointment_WithEmptyReason_WhenReasonIsNull()
        {
            // Arrange
            var appointment = CreateAppointment(status: AppointmentStatus.Pending);
            _appointmentRepositoryMock
                .Setup(r => r.GetByIdAsync(appointment.AppointmentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointment);

            _appointmentRepositoryMock
                .Setup(r => r.UpdateAsync(appointment.AppointmentId, It.IsAny<Appointment>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((int id, Appointment a, CancellationToken _) => a);

            _mapperMock
                .Setup(m => m.Map<AppointmentDto>(It.IsAny<Appointment>()))
                .Returns(new AppointmentDto { AppointmentId = appointment.AppointmentId, Status = AppointmentStatus.Cancelled });

            var dto = new UpdateAppointmentStatusDto
            {
                Status = AppointmentStatus.Cancelled,
                CancellationReason = null
            };

            // Act
            await _sut.UpdateStatusAsync(appointment.AppointmentId, dto);

            // Assert
            Assert.Equal(string.Empty, appointment.CancellationReason);
        }

        [Fact]
        public async Task UpdateStatusAsync_CompletesAppointment_WhenTransitioningFromConfirmedToCompleted()
        {
            // Arrange
            var appointment = CreateAppointment(status: AppointmentStatus.Confirmed);
            _appointmentRepositoryMock
                .Setup(r => r.GetByIdAsync(appointment.AppointmentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointment);

            _appointmentRepositoryMock
                .Setup(r => r.UpdateAsync(appointment.AppointmentId, It.IsAny<Appointment>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((int id, Appointment a, CancellationToken _) => a);

            _mapperMock
                .Setup(m => m.Map<AppointmentDto>(It.IsAny<Appointment>()))
                .Returns(new AppointmentDto { AppointmentId = appointment.AppointmentId, Status = AppointmentStatus.Completed });

            var dto = new UpdateAppointmentStatusDto { Status = AppointmentStatus.Completed };

            // Act
            await _sut.UpdateStatusAsync(appointment.AppointmentId, dto);

            // Assert
            Assert.Equal(AppointmentStatus.Completed, appointment.Status);
        }

        // =====================================================================
        // DeleteAsync
        // =====================================================================

        [Fact]
        public async Task DeleteAsync_ThrowsNotFoundException_WhenAppointmentDoesNotExist()
        {
            // Arrange
            _appointmentRepositoryMock
                .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Appointment?)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _sut.DeleteAsync(1));
            Assert.Equal("Appointment not found.", ex.Message);
        }

        [Fact]
        public async Task DeleteAsync_ThrowsValidationException_WhenAppointmentIsCompleted()
        {
            // Arrange
            var appointment = CreateAppointment(status: AppointmentStatus.Completed);
            _appointmentRepositoryMock
                .Setup(r => r.GetByIdAsync(appointment.AppointmentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointment);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ValidationException>(() => _sut.DeleteAsync(appointment.AppointmentId));
            Assert.Equal("Completed appointments cannot be deleted.", ex.Message);

            _appointmentRepositoryMock.Verify(
                r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_ThrowsValidationException_WhenAppointmentIsConfirmed()
        {
            // Arrange
            var appointment = CreateAppointment(status: AppointmentStatus.Confirmed);
            _appointmentRepositoryMock
                .Setup(r => r.GetByIdAsync(appointment.AppointmentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointment);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ValidationException>(() => _sut.DeleteAsync(appointment.AppointmentId));
            Assert.Equal("Confirmed appointments cannot be deleted.", ex.Message);
        }

        [Theory]
        [InlineData(AppointmentStatus.Pending)]
        [InlineData(AppointmentStatus.Cancelled)]
        public async Task DeleteAsync_DeletesAppointment_WhenStatusAllowsDeletion(AppointmentStatus status)
        {
            // Arrange
            var appointment = CreateAppointment(status: status);
            var expectedDto = new AppointmentDto { AppointmentId = appointment.AppointmentId, Status = status };

            _appointmentRepositoryMock
                .Setup(r => r.GetByIdAsync(appointment.AppointmentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointment);

            _appointmentRepositoryMock
                .Setup(r => r.DeleteAsync(appointment.AppointmentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointment);

            _mapperMock
                .Setup(m => m.Map<AppointmentDto>(appointment))
                .Returns(expectedDto);

            // Act
            var result = await _sut.DeleteAsync(appointment.AppointmentId);

            // Assert
            Assert.Equal(expectedDto, result);
            _appointmentRepositoryMock.Verify(
                r => r.DeleteAsync(appointment.AppointmentId, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        // =====================================================================
        // CancelAppointmentByPatientAsync
        // =====================================================================

        [Fact]
        public async Task CancelAppointmentByPatientAsync_ThrowsNotFoundException_WhenAppointmentDoesNotExist()
        {
            // Arrange
            _appointmentRepositoryMock
                .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Appointment?)null);

            var dto = new CancelAppointmentDto { CancellationReason = "Change of plans" };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<NotFoundException>(
                () => _sut.CancelAppointmentByPatientAsync(patientId: 1, appointmentId: 1, dto));
            Assert.Equal("Appointment not found.", ex.Message);
        }

        [Fact]
        public async Task CancelAppointmentByPatientAsync_ThrowsValidationException_WhenAppointmentBelongsToAnotherPatient()
        {
            // Arrange
            var appointment = CreateAppointment(patientId: 5, status: AppointmentStatus.Pending);
            _appointmentRepositoryMock
                .Setup(r => r.GetByIdAsync(appointment.AppointmentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointment);

            var dto = new CancelAppointmentDto { CancellationReason = "Change of plans" };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ValidationException>(
                () => _sut.CancelAppointmentByPatientAsync(patientId: 999, appointmentId: appointment.AppointmentId, dto));
            Assert.Equal("This appointment does not belong to the patient.", ex.Message);
        }

        [Fact]
        public async Task CancelAppointmentByPatientAsync_ThrowsValidationException_WhenAppointmentIsNotPending()
        {
            // Arrange
            var appointment = CreateAppointment(patientId: 1, status: AppointmentStatus.Confirmed);
            _appointmentRepositoryMock
                .Setup(r => r.GetByIdAsync(appointment.AppointmentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointment);

            var dto = new CancelAppointmentDto { CancellationReason = "Change of plans" };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ValidationException>(
                () => _sut.CancelAppointmentByPatientAsync(patientId: 1, appointmentId: appointment.AppointmentId, dto));
            Assert.Equal("Only pending appointments can be cancelled.", ex.Message);
        }

        [Fact]
        public async Task CancelAppointmentByPatientAsync_CancelsAppointment_WhenPendingAndOwnedByPatient()
        {
            // Arrange
            var appointment = CreateAppointment(patientId: 1, status: AppointmentStatus.Pending);
            var dto = new CancelAppointmentDto { CancellationReason = "No longer needed" };
            var expectedDto = new AppointmentDto
            {
                AppointmentId = appointment.AppointmentId,
                Status = AppointmentStatus.Cancelled,
                CancellationReason = dto.CancellationReason
            };

            _appointmentRepositoryMock
                .Setup(r => r.GetByIdAsync(appointment.AppointmentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointment);

            _appointmentRepositoryMock
                .Setup(r => r.UpdateAsync(appointment.AppointmentId, It.IsAny<Appointment>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((int id, Appointment a, CancellationToken _) => a);

            _mapperMock
                .Setup(m => m.Map<AppointmentDto>(It.Is<Appointment>(a =>
                    a.Status == AppointmentStatus.Cancelled &&
                    a.CancellationReason == dto.CancellationReason)))
                .Returns(expectedDto);

            // Act
            var result = await _sut.CancelAppointmentByPatientAsync(
                patientId: 1,
                appointmentId: appointment.AppointmentId,
                dto);

            // Assert
            Assert.Equal(AppointmentStatus.Cancelled, appointment.Status);
            Assert.Equal(dto.CancellationReason, appointment.CancellationReason);
            Assert.Equal(expectedDto, result);

            _appointmentRepositoryMock.Verify(
                r => r.UpdateAsync(appointment.AppointmentId, It.IsAny<Appointment>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}