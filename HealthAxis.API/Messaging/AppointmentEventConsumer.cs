using HealthAxis.API.Events;
using HealthAxis.API.Services.Interfaces;
using MassTransit;
using Microsoft.Extensions.Caching.Distributed;

namespace HealthAxis.API.Messaging
{
    public partial class AppointmentEventConsumer : IConsumer<AppointmentEvent>
    {
        private readonly ILogger<AppointmentEventConsumer> _logger;
        private readonly INotificationService _notificationService;

        public AppointmentEventConsumer(
            ILogger<AppointmentEventConsumer> logger,
            INotificationService notificationService)

        {
            _logger = logger;
            _notificationService = notificationService;
        }

        [LoggerMessage(EventId = 1, Level = LogLevel.Information,
            Message = "Received AppointmentCreated event for AppointmentId: {AppointmentId}, EventId: {EventId}")]
        private static partial void LogAppointmentCreated(ILogger logger, int appointmentId, Guid eventId);

        [LoggerMessage(EventId = 2, Level = LogLevel.Information,
            Message = "Notification created successfully for DoctorId: {DoctorId}")]
        private static partial void LogNotificationCreated(ILogger logger, int doctorId);

        [LoggerMessage(EventId = 3, Level = LogLevel.Error,
            Message = "Failed to create notification for AppointmentId: {AppointmentId}")]
        private static partial void LogNotificationFailed(ILogger logger, Exception exception, int appointmentId);

        [LoggerMessage(EventId = 4, Level = LogLevel.Warning,
            Message = "Duplicate event {EventId} detected — skipping (already processed).")]
        private static partial void LogDuplicateSkipped(ILogger logger, Guid eventId);

        public async Task Consume(ConsumeContext<AppointmentEvent> context)
        {
            var message = context.Message;

            if (message.EventType != "AppointmentCreated")
                return;

            LogAppointmentCreated(_logger, message.AppointmentId, message.EventId);

            try
            {
                await _notificationService.CreateNotificationAsync(
                    message.DoctorId,
                    "New Appointment",
                    $"A new appointment (ID: {message.AppointmentId}) has been booked.");


                LogNotificationCreated(_logger, message.DoctorId);
            }
            catch (Exception ex)
            {
                LogNotificationFailed(_logger, ex, message.AppointmentId);
                throw; 
            }
        }
    }
}