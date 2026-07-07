using System.Text;
using System.Text.Json;
using HealthAxis.API.Events;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace HealthAxis.API.Services
{
    public class HeartbeatService : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<HeartbeatService> _logger;

        private IConnection? _connection;
        private IChannel? _channel;

        public HeartbeatService(
            IConfiguration configuration,
            ILogger<HeartbeatService> logger)
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

            await _channel.QueueDeclareAsync(
                queue: "heartbeat-queue",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: cancellationToken);

            _logger.LogInformation("Heartbeat Service Started.");

            await base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            if (_channel == null)
                return;

            while (!stoppingToken.IsCancellationRequested)
            {
                var heartbeat = new HeartbeatEvent
                {
                    ServiceName = "HealthAxis.API",
                    Status = "Running",
                    Timestamp = DateTime.UtcNow,
                    MachineName = Environment.MachineName
                };

                var json = JsonSerializer.Serialize(heartbeat);

                var body = Encoding.UTF8.GetBytes(json);

                var properties = new BasicProperties
                {
                    Persistent = true
                };

                await _channel.BasicPublishAsync(
                    exchange: string.Empty,
                    routingKey: "heartbeat-queue",
                    mandatory: false,
                    basicProperties: properties,
                    body: body,
                    cancellationToken: stoppingToken);

                _logger.LogInformation(
                    "Heartbeat sent at {Time}",
                    heartbeat.Timestamp);

                await Task.Delay(
                    TimeSpan.FromSeconds(10),
                    stoppingToken);
            }
        }

        public override async Task StopAsync(
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping Heartbeat Service...");

            if (_channel != null)
                await _channel.CloseAsync(cancellationToken);

            if (_connection != null)
                await _connection.CloseAsync(cancellationToken);

            await base.StopAsync(cancellationToken);
        }
    }
}