namespace HealthAxis.Shared.DTOs.NotificationDtos
{
    public class NotificationDto
    {
        public int NotificationId { get; set; }

        public int DoctorId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public bool IsRead { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}