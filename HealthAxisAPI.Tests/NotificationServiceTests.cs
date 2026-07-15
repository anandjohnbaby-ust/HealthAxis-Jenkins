using AutoMapper;
using FluentAssertions;
using HealthAxis.API.Exceptions;
using HealthAxis.API.Models;
using HealthAxis.API.Repositories.Interfaces;
using HealthAxis.API.Services.Implementations;
using HealthAxis.Shared.DTOs.NotificationDtos;
using Moq;
using Xunit;

namespace HealthAxis.Tests.Services
{
    public class NotificationServiceTests
    {
        protected readonly Mock<INotificationRepository> _notificationRepository;
        protected readonly Mock<IMapper> _mapper;
        protected readonly NotificationService _service;

        public NotificationServiceTests()
        {
            _notificationRepository = new Mock<INotificationRepository>();
            _mapper = new Mock<IMapper>();

            _service = new NotificationService(
                _notificationRepository.Object,
                _mapper.Object);
        }

        #region CreateNotificationAsync

        [Fact]
        public async Task CreateNotificationAsync_ShouldCreateAndReturnNotification_WhenCalled()
        {
            var doctorId = 1;
            var title = "New Appointment";
            var message = "You have a new appointment request.";

            var savedNotification = new Notification
            {
                NotificationId = 10,
                DoctorId = doctorId,
                Title = title,
                Message = message,
                IsRead = false
            };

            var notificationDto = new NotificationDto
            {
                NotificationId = 10,
                DoctorId = doctorId,
                Title = title,
                Message = message
            };

            _notificationRepository
                .Setup(x => x.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(savedNotification);

            _mapper
                .Setup(x => x.Map<NotificationDto>(savedNotification))
                .Returns(notificationDto);

            var result = await _service.CreateNotificationAsync(doctorId, title, message);

            result.Should().BeEquivalentTo(notificationDto);

            _notificationRepository.Verify(
                x => x.AddAsync(
                    It.Is<Notification>(n =>
                        n.DoctorId == doctorId &&
                        n.Title == title &&
                        n.Message == message &&
                        n.IsRead == false),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task CreateNotificationAsync_ShouldSetIsReadFalse_AndCreatedAtToUtcNow()
        {
            var beforeCall = DateTime.UtcNow;

            Notification? capturedNotification = null;

            _notificationRepository
                .Setup(x => x.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
                .Callback<Notification, CancellationToken>((n, _) => capturedNotification = n)
                .ReturnsAsync((Notification n, CancellationToken _) => n);

            _mapper
                .Setup(x => x.Map<NotificationDto>(It.IsAny<Notification>()))
                .Returns(new NotificationDto());

            await _service.CreateNotificationAsync(1, "Title", "Message");

            var afterCall = DateTime.UtcNow;

            capturedNotification.Should().NotBeNull();
            capturedNotification!.IsRead.Should().BeFalse();
            capturedNotification.CreatedAt.Should().BeOnOrAfter(beforeCall);
            capturedNotification.CreatedAt.Should().BeOnOrBefore(afterCall);
        }

        [Fact]
        public async Task CreateNotificationAsync_ShouldThrowException_WhenRepositoryThrows()
        {
            _notificationRepository
                .Setup(x => x.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database Error"));

            Func<Task> act = async () =>
                await _service.CreateNotificationAsync(1, "Title", "Message");

            await act.Should().ThrowAsync<Exception>().WithMessage("Database Error");
        }

        #endregion

        #region GetDoctorNotificationsAsync

        [Fact]
        public async Task GetDoctorNotificationsAsync_ShouldReturnNotifications_WhenRepositoryReturnsData()
        {
            var doctorId = 1;

            var notifications = new List<Notification>
            {
                new() { NotificationId = 1, DoctorId = doctorId },
                new() { NotificationId = 2, DoctorId = doctorId }
            };

            var notificationDtos = new List<NotificationDto>
            {
                new() { NotificationId = 1, DoctorId = doctorId },
                new() { NotificationId = 2, DoctorId = doctorId }
            };

            _notificationRepository
                .Setup(x => x.GetDoctorNotificationsAsync(doctorId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(notifications);

            _mapper
                .Setup(x => x.Map<IEnumerable<NotificationDto>>(notifications))
                .Returns(notificationDtos);

            var result = await _service.GetDoctorNotificationsAsync(doctorId);

            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().BeEquivalentTo(notificationDtos);
        }

        [Fact]
        public async Task GetDoctorNotificationsAsync_ShouldReturnEmptyCollection_WhenNoNotificationsExist()
        {
            var doctorId = 1;

            var notifications = new List<Notification>();
            var notificationDtos = new List<NotificationDto>();

            _notificationRepository
                .Setup(x => x.GetDoctorNotificationsAsync(doctorId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(notifications);

            _mapper
                .Setup(x => x.Map<IEnumerable<NotificationDto>>(notifications))
                .Returns(notificationDtos);

            var result = await _service.GetDoctorNotificationsAsync(doctorId);

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetDoctorNotificationsAsync_ShouldThrowException_WhenRepositoryThrows()
        {
            _notificationRepository
                .Setup(x => x.GetDoctorNotificationsAsync(1, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database Error"));

            Func<Task> act = async () => await _service.GetDoctorNotificationsAsync(1);

            await act.Should().ThrowAsync<Exception>().WithMessage("Database Error");
        }

        #endregion

        #region MarkAsReadAsync

        [Fact]
        public async Task MarkAsReadAsync_ShouldCompleteSuccessfully_WhenNotificationIsMarkedRead()
        {
            _notificationRepository
                .Setup(x => x.MarkAsReadAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            Func<Task> act = async () => await _service.MarkAsReadAsync(1);

            await act.Should().NotThrowAsync();

            _notificationRepository.Verify(
                x => x.MarkAsReadAsync(1, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task MarkAsReadAsync_ShouldThrowNotFoundException_WhenNotificationDoesNotExist()
        {
            _notificationRepository
                .Setup(x => x.MarkAsReadAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            Func<Task> act = async () => await _service.MarkAsReadAsync(1);

            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Notification not found.");
        }

        #endregion
    }
}