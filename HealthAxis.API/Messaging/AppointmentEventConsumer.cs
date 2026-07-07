using System.Text;
using System.Text.Json;
using HealthAxis.API.Events;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace HealthAxis.API.Messaging
{
    public class AppointmentEventConsumer : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AppointmentEventConsumer> _logger;

        private IConnection? _connection;
        private IChannel? _channel;

        public AppointmentEventConsumer(
            IConfiguration configuration,
            ILogger<AppointmentEventConsumer> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            var rabbitConfig = _configuration.GetSection("RabbitMQ");

            var factory = new ConnectionFactory
            {
                HostName = rabbitConfig["HostName"],
                Port = int.Parse(rabbitConfig["Port"]!),
                UserName = rabbitConfig["UserName"],
                Password = rabbitConfig["Password"],
                VirtualHost = rabbitConfig["VirtualHost"]
            };

            _connection = await factory.CreateConnectionAsync(cancellationToken);

            _channel = await _connection.CreateChannelAsync(
                cancellationToken: cancellationToken);

            _logger.LogInformation(
                "AppointmentEventConsumer started and connected to RabbitMQ.");

            await base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            if (_channel == null)
                return;

            var queueName =
                _configuration.GetSection("RabbitMQ")["AppointmentQueue"]!;

            await _channel.QueueDeclareAsync(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: stoppingToken);

            var consumer =
                new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (sender, eventArgs) =>
            {
                try
                {
                    var body = eventArgs.Body.ToArray();

                    var json = Encoding.UTF8.GetString(body);

                    var appointmentEvent =
                        JsonSerializer.Deserialize<AppointmentEvent>(
                            json,
                            new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });

                    if (appointmentEvent != null)
                    {
                        _logger.LogInformation(
                            """
                            ===========================
                            APPOINTMENT EVENT RECEIVED
                            ===========================
                            Event Type   : {EventType}
                            Appointment  : {AppointmentId}
                            Patient      : {PatientId}
                            Doctor       : {DoctorId}
                            Time         : {OccurredAt}
                            ===========================
                            """,
                            appointmentEvent.EventType,
                            appointmentEvent.AppointmentId,
                            appointmentEvent.PatientId,
                            appointmentEvent.DoctorId,
                            appointmentEvent.OccurredAt);
                    }

                    await _channel.BasicAckAsync(
                        eventArgs.DeliveryTag,
                        false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Error while processing appointment event.");

                    await _channel.BasicNackAsync(
                        eventArgs.DeliveryTag,
                        false,
                        true);
                }
            };

            await _channel.BasicConsumeAsync(
                queue: queueName,
                autoAck: false,
                consumer: consumer,
                cancellationToken: stoppingToken);

            await Task.Delay(
                Timeout.Infinite,
                stoppingToken);
        }

        public override async Task StopAsync(
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Stopping AppointmentEventConsumer...");

            if (_channel != null)
                await _channel.CloseAsync(cancellationToken);

            if (_connection != null)
                await _connection.CloseAsync(cancellationToken);

            await base.StopAsync(cancellationToken);
        }
    }
}