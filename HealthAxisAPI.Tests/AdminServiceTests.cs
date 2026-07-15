using FluentAssertions;
using HealthAxis.API.Repositories.Interfaces;
using HealthAxis.API.Services.Implementations;
using HealthAxis.Shared.Common;
using HealthAxis.Shared.DTOs.AdminDtos;
using Moq;
using Xunit;

namespace HealthAxis.Tests.Services
{
    public class AdminServiceTests
    {
        private readonly Mock<IAdminRepository> _adminRepositoryMock;
        private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock;

        private readonly AdminService _service;

        public AdminServiceTests()
        {
            _adminRepositoryMock =
                new Mock<IAdminRepository>();

            _appointmentRepositoryMock =
                new Mock<IAppointmentRepository>();

            _service =
                new AdminService(
                    _adminRepositoryMock.Object,
                    _appointmentRepositoryMock.Object);
        }

        #region Constructor

        [Fact]
        public void Constructor_ShouldCreateInstance_WhenDependenciesAreProvided()
        {
            // Arrange & Act

            var service = new AdminService(
                _adminRepositoryMock.Object,
                _appointmentRepositoryMock.Object);

            // Assert

            service.Should().NotBeNull();
        }

        #endregion

        #region GetDashboardAsync

        [Fact]
        public async Task GetDashboardAsync_ShouldReturnDashboard_WhenRepositoryReturnsData()
        {
            // Arrange

            var dashboard = new DashboardDto
            {
                TotalDoctors = 10,
                TotalPatients = 20,
                TotalAppointments = 30
            };

            _adminRepositoryMock
                .Setup(x => x.GetDashboardAsync())
                .ReturnsAsync(dashboard);

            // Act

            var result = await _service.GetDashboardAsync();

            // Assert

            result.Should().NotBeNull();

            result.Should().BeEquivalentTo(dashboard);

            _adminRepositoryMock.Verify(
                x => x.GetDashboardAsync(),
                Times.Once);

            _adminRepositoryMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetDashboardAsync_ShouldReturnEmptyDashboard_WhenRepositoryReturnsEmptyObject()
        {
            // Arrange

            var dashboard = new DashboardDto();

            _adminRepositoryMock
                .Setup(x => x.GetDashboardAsync())
                .ReturnsAsync(dashboard);

            // Act

            var result = await _service.GetDashboardAsync();

            // Assert

            result.Should().NotBeNull();

            result.Should().BeEquivalentTo(dashboard);

            _adminRepositoryMock.Verify(
                x => x.GetDashboardAsync(),
                Times.Once);

            _adminRepositoryMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetDashboardAsync_ShouldThrowException_WhenRepositoryThrowsException()
        {
            // Arrange

            _adminRepositoryMock
                .Setup(x => x.GetDashboardAsync())
                .ThrowsAsync(new Exception("Database Error"));

            // Act

            Func<Task> act = async () => await _service.GetDashboardAsync();

            // Assert

            await act.Should()
                .ThrowAsync<Exception>()
                .WithMessage("Database Error");
        }

        #endregion

        #region GetAppointmentReportAsync

        [Fact]
        public async Task GetAppointmentReportAsync_ShouldReturnPagedReport_WhenRepositoryReturnsData()
        {
            // Arrange

            var request = new PaginationRequest { PageNumber = 1, PageSize = 10 };

            var pagedReport = new PagedResult<AppointmentReportDto>
            {
                Items = new List<AppointmentReportDto>
        {
            new()
            {
                Date = new DateOnly(2026, 7, 14),
                ConfirmedCount = 5,
                PendingCount = 2,
                CancelledCount = 1,
                CompletedCount = 8,
                TotalAppointments = 16
            },
            new()
            {
                Date = new DateOnly(2026, 7, 15),
                ConfirmedCount = 3,
                PendingCount = 1,
                CancelledCount = 0,
                CompletedCount = 4,
                TotalAppointments = 8
            }
        },
                TotalCount = 2,
                PageNumber = 1,
                PageSize = 10
            };

            _appointmentRepositoryMock
                .Setup(x => x.GetAppointmentReportAsync(request))
                .ReturnsAsync(pagedReport);

            // Act

            var result = await _service.GetAppointmentReportAsync(request);

            // Assert

            result.Should().NotBeNull();

            result.Should().BeEquivalentTo(pagedReport);

            result.Items.Should().HaveCount(2);

            _appointmentRepositoryMock.Verify(
                x => x.GetAppointmentReportAsync(request),
                Times.Once);

            _appointmentRepositoryMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetAppointmentReportAsync_ShouldReturnEmptyResult_WhenNoAppointmentsExist()
        {
            // Arrange

            var request = new PaginationRequest { PageNumber = 1, PageSize = 10 };

            var pagedReport = new PagedResult<AppointmentReportDto>
            {
                Items = new List<AppointmentReportDto>(),
                TotalCount = 0,
                PageNumber = 1,
                PageSize = 10
            };

            _appointmentRepositoryMock
                .Setup(x => x.GetAppointmentReportAsync(request))
                .ReturnsAsync(pagedReport);

            // Act

            var result = await _service.GetAppointmentReportAsync(request);

            // Assert

            result.Should().NotBeNull();

            result.Items.Should().BeEmpty();

            result.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task GetAppointmentReportAsync_ShouldThrowException_WhenRepositoryThrowsException()
        {
            // Arrange

            var request = new PaginationRequest { PageNumber = 1, PageSize = 10 };

            _appointmentRepositoryMock
                .Setup(x => x.GetAppointmentReportAsync(request))
                .ThrowsAsync(new Exception("Database Error"));

            // Act

            Func<Task> act = async () => await _service.GetAppointmentReportAsync(request);

            // Assert

            await act.Should()
                .ThrowAsync<Exception>()
                .WithMessage("Database Error");
        }

        #endregion
    }
}