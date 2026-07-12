using AutoMapper;
using HealthAxis.API.Exceptions;
using HealthAxis.API.Models;
using HealthAxis.API.Repositories.Interfaces;
using HealthAxis.API.Services.Interfaces;
using HealthAxis.Shared.DTOs.NotificationDtos;

namespace HealthAxis.API.Services.Implementations
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IMapper _mapper;

        public NotificationService(
            INotificationRepository notificationRepository,
            IMapper mapper)
        {
            _notificationRepository = notificationRepository;
            _mapper = mapper;
        }

        public async Task<NotificationDto> CreateNotificationAsync(
            int doctorId,
            string title,
            string message,
            CancellationToken ct = default)
        {
            var notification = new Notification
            {
                DoctorId = doctorId,
                Title = title,
                Message = message,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            var savedNotification = await _notificationRepository.AddAsync(
                notification,
                ct);

            return _mapper.Map<NotificationDto>(savedNotification);
        }

        public async Task<IEnumerable<NotificationDto>> GetDoctorNotificationsAsync(
            int doctorId,
            CancellationToken ct = default)
        {
            var notifications =
                await _notificationRepository.GetDoctorNotificationsAsync(
                    doctorId,
                    ct);

            return _mapper.Map<IEnumerable<NotificationDto>>(
                notifications);
        }

        public async Task MarkAsReadAsync(
            int notificationId,
            CancellationToken ct = default)
        {
            var updated =
                await _notificationRepository.MarkAsReadAsync(
                    notificationId,
                    ct);

            if (!updated)
            {
                throw new NotFoundException(
                    "Notification not found.");
            }
        }
    }
}