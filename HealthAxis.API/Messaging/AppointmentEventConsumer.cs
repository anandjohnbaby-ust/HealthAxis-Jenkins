using HealthAxis.API.Events;
using HealthAxis.API.Services.Interfaces;
using MassTransit;

namespace HealthAxis.API.Messaging
{
    public class AppointmentEventConsumer : IConsumer<AppointmentEvent>
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

        public async Task Consume(
            ConsumeContext<AppointmentEvent> context)
        {
            var message = context.Message;

            _logger.LogInformation(
                "Received Appointment Event: {EventType}, AppointmentId: {AppointmentId}",
                message.EventType,
                message.AppointmentId);

            string title;
            string body;

            switch (message.EventType)
            {
                case "AppointmentCreated":

                    title = "New Appointment";

                    body = $"A new appointment (ID: {message.AppointmentId}) has been booked.";

                    break;

                case "Confirmed":

                    title = "Appointment Confirmed";

                    body = $"Appointment #{message.AppointmentId} has been confirmed.";

                    break;

                case "Completed":

                    title = "Appointment Completed";

                    body = $"Appointment #{message.AppointmentId} has been completed.";

                    break;

                case "Cancelled":

                case "PatientCancelled":

                    title = "Appointment Cancelled";

                    body = $"Appointment #{message.AppointmentId} has been cancelled.";

                    break;

                case "AppointmentDeleted":

                    title = "Appointment Deleted";

                    body = $"Appointment #{message.AppointmentId} has been deleted.";

                    break;

                default:

                    title = "Appointment Updated";

                    body = $"Appointment #{message.AppointmentId} has been updated.";

                    break;
            }

            try
            {
                await _notificationService.CreateNotificationAsync(
                    message.DoctorId,
                    title,
                    body);

                _logger.LogInformation(
                    "Notification created successfully for DoctorId: {DoctorId}",
                    message.DoctorId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to create notification for AppointmentId: {AppointmentId}",
                    message.AppointmentId);

                throw;
            }
        }
    }
}