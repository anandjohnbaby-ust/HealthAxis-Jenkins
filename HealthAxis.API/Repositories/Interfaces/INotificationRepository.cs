using HealthAxis.API.Models;

namespace HealthAxis.API.Repositories.Interfaces
{
    public interface INotificationRepository : IRepository<Notification>
    {
        Task<List<Notification>> GetDoctorNotificationsAsync(
            int doctorId,
            CancellationToken ct = default);

        Task<List<Notification>> GetUnreadNotificationsAsync(
            int doctorId,
            CancellationToken ct = default);

        Task<bool> MarkAsReadAsync(
            int notificationId,
            CancellationToken ct = default);
    }
}