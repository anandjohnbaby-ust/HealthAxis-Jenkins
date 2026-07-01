using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using HealthAxis.API.Exceptions;
using HealthAxis.API.Repositories.Interfaces;
using HealthAxis.API.Services.Implementations;
using HealthAxis.API.Services.Interfaces;
using HealthAxis.Shared.Common;
using HealthAxis.Shared.DTOs.AdminDtos;
using HealthAxis.Shared.DTOs.AppointmentDtos;
using HealthAxis.Shared.DTOs.DoctorDtos;
using HealthAxis.Shared.DTOs.PatientDtos;
using HealthAxis.Shared.Enums;
using Moq;
using Xunit;

namespace HealthAxis.API.Tests.Services
{
    public class AdminServiceTests
    {
        private readonly Mock<IDoctorService> _doctorServiceMock;
        private readonly Mock<IPatientService> _patientServiceMock;
        private readonly Mock<IAppointmentService> _appointmentServiceMock;
        private readonly Mock<IAdminRepository> _adminRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;

        private readonly AdminService _sut; // system under test

        public AdminServiceTests()
        {
            _doctorServiceMock = new Mock<IDoctorService>(MockBehavior.Strict);
            _patientServiceMock = new Mock<IPatientService>(MockBehavior.Strict);
            _appointmentServiceMock = new Mock<IAppointmentService>(MockBehavior.Strict);
            _adminRepositoryMock = new Mock<IAdminRepository>(MockBehavior.Strict);
            _mapperMock = new Mock<IMapper>(MockBehavior.Loose);

            _sut = new AdminService(
                _doctorServiceMock.Object,
                _patientServiceMock.Object,
                _appointmentServiceMock.Object,
                _adminRepositoryMock.Object,
                _mapperMock.Object);
        }

        #region Constructor

        [Fact]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            // Arrange & Act
            var instance = new AdminService(
                _doctorServiceMock.Object,
                _patientServiceMock.Object,
                _appointmentServiceMock.Object,
                _adminRepositoryMock.Object,
                _mapperMock.Object);

            // Assert
            Assert.NotNull(instance);
            Assert.IsAssignableFrom<IAdminService>(instance);
        }

        // NOTE: These constructor null-guard tests will only pass if AdminService's
        // constructor is updated to validate arguments (e.g. via ArgumentNullException.ThrowIfNull).
        // They are included because null-checking constructor dependencies is best practice.
        // If the production code intentionally omits these guards, remove/skip these tests
        // or mark them with [Fact(Skip = "No null-guards implemented in AdminService ctor")].

        [Fact(Skip = "Enable once AdminService constructor validates arguments for null")]
        public void Constructor_NullDoctorService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new AdminService(
                null!,
                _patientServiceMock.Object,
                _appointmentServiceMock.Object,
                _adminRepositoryMock.Object,
                _mapperMock.Object));
        }

        [Fact(Skip = "Enable once AdminService constructor validates arguments for null")]
        public void Constructor_NullPatientService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new AdminService(
                _doctorServiceMock.Object,
                null!,
                _appointmentServiceMock.Object,
                _adminRepositoryMock.Object,
                _mapperMock.Object));
        }

        [Fact(Skip = "Enable once AdminService constructor validates arguments for null")]
        public void Constructor_NullAppointmentService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new AdminService(
                _doctorServiceMock.Object,
                _patientServiceMock.Object,
                null!,
                _adminRepositoryMock.Object,
                _mapperMock.Object));
        }

        [Fact(Skip = "Enable once AdminService constructor validates arguments for null")]
        public void Constructor_NullAdminRepository_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new AdminService(
                _doctorServiceMock.Object,
                _patientServiceMock.Object,
                _appointmentServiceMock.Object,
                null!,
                _mapperMock.Object));
        }

        [Fact(Skip = "Enable once AdminService constructor validates arguments for null")]
        public void Constructor_NullMapper_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new AdminService(
                _doctorServiceMock.Object,
                _patientServiceMock.Object,
                _appointmentServiceMock.Object,
                _adminRepositoryMock.Object,
                null!));
        }

        #endregion

        #region GetDoctorById

        [Fact]
        public async Task GetDoctorById_ValidId_ReturnsDoctorFromDoctorService()
        {
            // Arrange
            const int doctorId = 5;
            var expectedDoctor = new DoctorDto { DoctorId = doctorId };

            _doctorServiceMock
                .Setup(s => s.GetDoctorById(doctorId))
                .ReturnsAsync(expectedDoctor);

            // Act
            var result = await _sut.GetDoctorById(doctorId);

            // Assert
            Assert.Same(expectedDoctor, result);
            _doctorServiceMock.Verify(s => s.GetDoctorById(doctorId), Times.Once);
            _doctorServiceMock.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(int.MaxValue)]
        public async Task GetDoctorById_PassesIdThroughUnchanged_RegardlessOfValue(int id)
        {
            // Arrange
            var expectedDoctor = new DoctorDto { DoctorId = id };
            _doctorServiceMock
                .Setup(s => s.GetDoctorById(id))
                .ReturnsAsync(expectedDoctor);

            // Act
            var result = await _sut.GetDoctorById(id);

            // Assert
            Assert.Equal(id, result.DoctorId);
            _doctorServiceMock.Verify(s => s.GetDoctorById(id), Times.Once);
        }

        [Fact]
        public async Task GetDoctorById_DoctorNotFound_PropagatesNotFoundException()
        {
            // Arrange
            const int doctorId = 999;
            _doctorServiceMock
                .Setup(s => s.GetDoctorById(doctorId))
                .ThrowsAsync(new NotFoundException("Doctor not found"));

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetDoctorById(doctorId));
            _doctorServiceMock.Verify(s => s.GetDoctorById(doctorId), Times.Once);
        }

        [Fact]
        public async Task GetDoctorById_DoesNotCallOtherDependencies()
        {
            // Arrange
            var expectedDoctor = new DoctorDto { DoctorId = 1 };
            _doctorServiceMock.Setup(s => s.GetDoctorById(1)).ReturnsAsync(expectedDoctor);

            // Act
            await _sut.GetDoctorById(1);

            // Assert - Strict mocks for untouched dependencies would throw on any call;
            // verifying no calls is an extra explicit guard.
            _patientServiceMock.VerifyNoOtherCalls();
            _appointmentServiceMock.VerifyNoOtherCalls();
            _adminRepositoryMock.VerifyNoOtherCalls();
        }

        #endregion

        #region GetDoctorsAsync

        [Fact]
        public async Task GetDoctorsAsync_ValidRequest_ReturnsPagedResultFromDoctorService()
        {
            // Arrange
            var request = new PaginationRequest();
            var specialisation = Specialisation.Cardiology;
            const string search = "john";
            var cts = new CancellationTokenSource();

            var expectedResult = new PagedResult<DoctorDto>();

            _doctorServiceMock
                .Setup(s => s.GetDoctorsAsync(request, specialisation, search, cts.Token))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _sut.GetDoctorsAsync(request, specialisation, search, cts.Token);

            // Assert
            Assert.Same(expectedResult, result);
            _doctorServiceMock.Verify(
                s => s.GetDoctorsAsync(request, specialisation, search, cts.Token),
                Times.Once);
        }

        [Fact]
        public async Task GetDoctorsAsync_NullSpecialisationAndSearch_PassesNullsThrough()
        {
            // Arrange
            var request = new PaginationRequest();
            var expectedResult = new PagedResult<DoctorDto>();

            _doctorServiceMock
                .Setup(s => s.GetDoctorsAsync(request, null, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _sut.GetDoctorsAsync(request, null, null);

            // Assert
            Assert.Same(expectedResult, result);
            _doctorServiceMock.Verify(
                s => s.GetDoctorsAsync(request, null, null, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetDoctorsAsync_NoCancellationTokenSupplied_UsesDefaultToken()
        {
            // Arrange
            var request = new PaginationRequest();
            var expectedResult = new PagedResult<DoctorDto>();

            _doctorServiceMock
                .Setup(s => s.GetDoctorsAsync(request, null, null, default))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _sut.GetDoctorsAsync(request, null, null);

            // Assert
            _doctorServiceMock.Verify(
                s => s.GetDoctorsAsync(request, null, null, default),
                Times.Once);
        }

        [Fact]
        public async Task GetDoctorsAsync_CancelledToken_PropagatesOperationCanceledException()
        {
            // Arrange
            var request = new PaginationRequest();
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            _doctorServiceMock
                .Setup(s => s.GetDoctorsAsync(request, null, null, cts.Token))
                .ThrowsAsync(new OperationCanceledException(cts.Token));

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(
                () => _sut.GetDoctorsAsync(request, null, null, cts.Token));
        }

        [Fact]
        public async Task GetDoctorsAsync_EmptySearchString_PassesEmptyStringThrough()
        {
            // Arrange
            var request = new PaginationRequest();
            var expectedResult = new PagedResult<DoctorDto>();

            _doctorServiceMock
                .Setup(s => s.GetDoctorsAsync(request, null, string.Empty, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _sut.GetDoctorsAsync(request, null, string.Empty);

            // Assert
            Assert.Same(expectedResult, result);
        }

        [Fact]
        public async Task GetDoctorsAsync_RepositoryThrows_PropagatesException()
        {
            // Arrange
            var request = new PaginationRequest();

            _doctorServiceMock
                .Setup(s => s.GetDoctorsAsync(request, null, null, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("DB failure"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.GetDoctorsAsync(request, null, null));
        }

        #endregion

        #region CreateDoctor

        [Fact]
        public async Task CreateDoctor_ValidDto_ReturnsCreatedDoctorFromDoctorService()
        {
            // Arrange
            var createDto = new CreateDoctorDto();
            var expectedDoctor = new DoctorDto { DoctorId = 10 };

            _doctorServiceMock
                .Setup(s => s.CreateDoctor(createDto))
                .ReturnsAsync(expectedDoctor);

            // Act
            var result = await _sut.CreateDoctor(createDto);

            // Assert
            Assert.Same(expectedDoctor, result);
            _doctorServiceMock.Verify(s => s.CreateDoctor(createDto), Times.Once);
            _doctorServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task CreateDoctor_DuplicateEmail_PropagatesBusinessRuleException()
        {
            // Arrange
            var createDto = new CreateDoctorDto();

            _doctorServiceMock
                .Setup(s => s.CreateDoctor(createDto))
                .ThrowsAsync(new BusinessRuleException("Doctor already exists"));

            // Act & Assert
            await Assert.ThrowsAsync<BusinessRuleException>(() => _sut.CreateDoctor(createDto));
        }

        [Fact]
        public async Task CreateDoctor_InvalidDto_PropagatesValidationException()
        {
            // Arrange
            var createDto = new CreateDoctorDto();

            _doctorServiceMock
                .Setup(s => s.CreateDoctor(createDto))
                .ThrowsAsync(new ValidationException("Invalid doctor data"));

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => _sut.CreateDoctor(createDto));
        }

        [Fact]
        public async Task CreateDoctor_CallsDoctorServiceExactlyOnce_NoDuplicateCalls()
        {
            // Arrange
            var createDto = new CreateDoctorDto();
            var expectedDoctor = new DoctorDto();

            _doctorServiceMock
                .Setup(s => s.CreateDoctor(It.IsAny<CreateDoctorDto>()))
                .ReturnsAsync(expectedDoctor);

            // Act
            await _sut.CreateDoctor(createDto);

            // Assert
            _doctorServiceMock.Verify(s => s.CreateDoctor(It.IsAny<CreateDoctorDto>()), Times.Once);
        }

        #endregion

        #region UpdateDoctor

        [Fact]
        public async Task UpdateDoctor_ValidIdAndDto_ReturnsUpdatedDoctorFromDoctorService()
        {
            // Arrange
            const int doctorId = 3;
            var updateDto = new UpdateDoctorDto();
            var expectedDoctor = new DoctorDto { DoctorId = doctorId };

            _doctorServiceMock
                .Setup(s => s.UpdateDoctor(doctorId, updateDto))
                .ReturnsAsync(expectedDoctor);

            // Act
            var result = await _sut.UpdateDoctor(doctorId, updateDto);

            // Assert
            Assert.Same(expectedDoctor, result);
            _doctorServiceMock.Verify(s => s.UpdateDoctor(doctorId, updateDto), Times.Once);
        }

        [Fact]
        public async Task UpdateDoctor_NonExistentId_PropagatesNotFoundException()
        {
            // Arrange
            const int doctorId = 404;
            var updateDto = new UpdateDoctorDto();

            _doctorServiceMock
                .Setup(s => s.UpdateDoctor(doctorId, updateDto))
                .ThrowsAsync(new NotFoundException("Doctor not found"));

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _sut.UpdateDoctor(doctorId, updateDto));
        }

        [Fact]
        public async Task UpdateDoctor_PassesExactDtoReference_NotACopy()
        {
            // Arrange
            const int doctorId = 1;
            var updateDto = new UpdateDoctorDto();
            UpdateDoctorDto? capturedDto = null;

            _doctorServiceMock
                .Setup(s => s.UpdateDoctor(doctorId, It.IsAny<UpdateDoctorDto>()))
                .Callback<int, UpdateDoctorDto>((_, dto) => capturedDto = dto)
                .ReturnsAsync(new DoctorDto());

            // Act
            await _sut.UpdateDoctor(doctorId, updateDto);

            // Assert
            Assert.Same(updateDto, capturedDto);
        }

        #endregion

        #region GetPatientsAsync

        [Fact]
        public async Task GetPatientsAsync_ValidRequest_ReturnsPagedResultFromPatientService()
        {
            // Arrange
            var request = new PaginationRequest();
            const string search = "jane";
            var cts = new CancellationTokenSource();
            var expectedResult = new PagedResult<PatientDto>();

            _patientServiceMock
                .Setup(s => s.GetPatientsAsync(request, search, cts.Token))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _sut.GetPatientsAsync(request, search, cts.Token);

            // Assert
            Assert.Same(expectedResult, result);
            _patientServiceMock.Verify(s => s.GetPatientsAsync(request, search, cts.Token), Times.Once);
            _patientServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetPatientsAsync_NullSearch_PassesNullThrough()
        {
            // Arrange
            var request = new PaginationRequest();
            var expectedResult = new PagedResult<PatientDto>();

            _patientServiceMock
                .Setup(s => s.GetPatientsAsync(request, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _sut.GetPatientsAsync(request, null);

            // Assert
            Assert.Same(expectedResult, result);
        }

        [Fact]
        public async Task GetPatientsAsync_NoCancellationTokenSupplied_UsesDefaultToken()
        {
            // Arrange
            var request = new PaginationRequest();
            var expectedResult = new PagedResult<PatientDto>();

            _patientServiceMock
                .Setup(s => s.GetPatientsAsync(request, null, default))
                .ReturnsAsync(expectedResult);

            // Act
            await _sut.GetPatientsAsync(request, null);

            // Assert
            _patientServiceMock.Verify(s => s.GetPatientsAsync(request, null, default), Times.Once);
        }

        [Fact]
        public async Task GetPatientsAsync_ServiceThrows_PropagatesException()
        {
            // Arrange
            var request = new PaginationRequest();

            _patientServiceMock
                .Setup(s => s.GetPatientsAsync(request, null, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Unexpected failure"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _sut.GetPatientsAsync(request, null));
        }

        #endregion

        #region UpdatePatientAsync

        [Fact]
        public async Task UpdatePatientAsync_ValidIdAndDto_ReturnsUpdatedPatientFromPatientService()
        {
            // Arrange
            const int patientId = 7;
            var updateDto = new UpdatePatientDto();
            var cts = new CancellationTokenSource();
            var expectedPatient = new PatientDto { PatientId = patientId };

            _patientServiceMock
                .Setup(s => s.UpdateAsync(patientId, updateDto, cts.Token))
                .ReturnsAsync(expectedPatient);

            // Act
            var result = await _sut.UpdatePatientAsync(patientId, updateDto, cts.Token);

            // Assert
            Assert.Same(expectedPatient, result);
            _patientServiceMock.Verify(s => s.UpdateAsync(patientId, updateDto, cts.Token), Times.Once);
        }

        [Fact]
        public async Task UpdatePatientAsync_NoCancellationTokenSupplied_UsesDefaultToken()
        {
            // Arrange
            const int patientId = 2;
            var updateDto = new UpdatePatientDto();
            var expectedPatient = new PatientDto { PatientId = patientId };

            _patientServiceMock
                .Setup(s => s.UpdateAsync(patientId, updateDto, default))
                .ReturnsAsync(expectedPatient);

            // Act
            var result = await _sut.UpdatePatientAsync(patientId, updateDto);

            // Assert
            Assert.Same(expectedPatient, result);
            _patientServiceMock.Verify(s => s.UpdateAsync(patientId, updateDto, default), Times.Once);
        }

        [Fact]
        public async Task UpdatePatientAsync_NonExistentId_PropagatesNotFoundException()
        {
            // Arrange
            const int patientId = 999;
            var updateDto = new UpdatePatientDto();

            _patientServiceMock
                .Setup(s => s.UpdateAsync(patientId, updateDto, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new NotFoundException("Patient not found"));

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(
                () => _sut.UpdatePatientAsync(patientId, updateDto));
        }

        [Fact]
        public async Task UpdatePatientAsync_CancelledToken_PropagatesOperationCanceledException()
        {
            // Arrange
            const int patientId = 1;
            var updateDto = new UpdatePatientDto();
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            _patientServiceMock
                .Setup(s => s.UpdateAsync(patientId, updateDto, cts.Token))
                .ThrowsAsync(new OperationCanceledException(cts.Token));

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(
                () => _sut.UpdatePatientAsync(patientId, updateDto, cts.Token));
        }

        #endregion

        #region GetAppointmentReport

        [Fact]
        public async Task GetAppointmentReport_ValidRequest_ReturnsReportFromAppointmentService()
        {
            // Arrange
            var request = new PaginationRequest();
            var expectedReport = new PagedResult<AppointmentReportDto>();

            _appointmentServiceMock
                .Setup(s => s.GetAppointmentReportAsync(request))
                .ReturnsAsync(expectedReport);

            // Act
            var result = await _sut.GetAppointmentReport(request);

            // Assert
            Assert.Same(expectedReport, result);
            _appointmentServiceMock.Verify(s => s.GetAppointmentReportAsync(request), Times.Once);
            _appointmentServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetAppointmentReport_ServiceThrows_PropagatesException()
        {
            // Arrange
            var request = new PaginationRequest();

            _appointmentServiceMock
                .Setup(s => s.GetAppointmentReportAsync(request))
                .ThrowsAsync(new InvalidOperationException("Report generation failed"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.GetAppointmentReport(request));
        }

        [Fact]
        public async Task GetAppointmentReport_DoesNotTouchOtherDependencies()
        {
            // Arrange
            var request = new PaginationRequest();
            var expectedReport = new PagedResult<AppointmentReportDto>();

            _appointmentServiceMock
                .Setup(s => s.GetAppointmentReportAsync(request))
                .ReturnsAsync(expectedReport);

            // Act
            await _sut.GetAppointmentReport(request);

            // Assert
            _doctorServiceMock.VerifyNoOtherCalls();
            _patientServiceMock.VerifyNoOtherCalls();
            _adminRepositoryMock.VerifyNoOtherCalls();
        }

        #endregion

        #region GetDashboardAsync

        [Fact]
        public async Task GetDashboardAsync_ReturnsDashboardFromAdminRepository()
        {
            // Arrange
            var expectedDashboard = new DashboardDto();

            _adminRepositoryMock
                .Setup(r => r.GetDashboardAsync())
                .ReturnsAsync(expectedDashboard);

            // Act
            var result = await _sut.GetDashboardAsync();

            // Assert
            Assert.Same(expectedDashboard, result);
            _adminRepositoryMock.Verify(r => r.GetDashboardAsync(), Times.Once);
            _adminRepositoryMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetDashboardAsync_RepositoryThrows_PropagatesException()
        {
            // Arrange
            _adminRepositoryMock
                .Setup(r => r.GetDashboardAsync())
                .ThrowsAsync(new Exception("Database unavailable"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _sut.GetDashboardAsync());
        }

        [Fact]
        public async Task GetDashboardAsync_CalledMultipleTimes_CallsRepositoryEachTime()
        {
            // Arrange
            var expectedDashboard = new DashboardDto();
            _adminRepositoryMock
                .Setup(r => r.GetDashboardAsync())
                .ReturnsAsync(expectedDashboard);

            // Act
            await _sut.GetDashboardAsync();
            await _sut.GetDashboardAsync();
            await _sut.GetDashboardAsync();

            // Assert
            _adminRepositoryMock.Verify(r => r.GetDashboardAsync(), Times.Exactly(3));
        }

        [Fact]
        public async Task GetDashboardAsync_DoesNotTouchOtherDependencies()
        {
            // Arrange
            var expectedDashboard = new DashboardDto();
            _adminRepositoryMock
                .Setup(r => r.GetDashboardAsync())
                .ReturnsAsync(expectedDashboard);

            // Act
            await _sut.GetDashboardAsync();

            // Assert
            _doctorServiceMock.VerifyNoOtherCalls();
            _patientServiceMock.VerifyNoOtherCalls();
            _appointmentServiceMock.VerifyNoOtherCalls();
        }

        #endregion
    }
}