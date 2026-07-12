using HealthAxis.Shared.DTOs.NotificationDtos;

namespace HealthAxis.API.Services.Interfaces
{
    public interface INotificationService
    {
        Task<NotificationDto> CreateNotificationAsync(
            int doctorId,
            string title,
            string message,
            CancellationToken ct = default);

        Task<IEnumerable<NotificationDto>> GetDoctorNotificationsAsync(
            int doctorId,
            CancellationToken ct = default);

        Task MarkAsReadAsync(
            int notificationId,
            CancellationToken ct = default);
    }
}