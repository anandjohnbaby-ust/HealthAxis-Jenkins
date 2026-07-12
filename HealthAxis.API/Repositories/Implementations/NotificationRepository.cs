using HealthAxis.API.Data;
using HealthAxis.API.Models;
using HealthAxis.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HealthAxis.API.Repositories.Implementations
{
    public class NotificationRepository
        : Repository<Notification>, INotificationRepository
    {
        public NotificationRepository(
            ApplicationDbContext context)
            : base(context)
        {
        }

        public async Task<List<Notification>> GetDoctorNotificationsAsync(
            int doctorId,
            CancellationToken ct = default)
        {
            return await _context.Notifications
                .AsNoTracking()
                .Where(n => n.DoctorId == doctorId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task<List<Notification>> GetUnreadNotificationsAsync(
            int doctorId,
            CancellationToken ct = default)
        {
            return await _context.Notifications
                .AsNoTracking()
                .Where(n =>
                    n.DoctorId == doctorId &&
                    !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task<bool> MarkAsReadAsync(
            int notificationId,
            CancellationToken ct = default)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(
                    n => n.NotificationId == notificationId,
                    ct);

            if (notification == null)
            {
                return false;
            }

            notification.IsRead = true;

            await _context.SaveChangesAsync(ct);

            return true;
        }
    }
}