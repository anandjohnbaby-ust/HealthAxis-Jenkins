using System.Text;
using System.Text.Json;
using HealthAxis.API.Events;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace HealthAxis.API.Services
{
    public partial class HeartbeatService : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<HeartbeatService> _logger;

        private IConnection? _connection;
        private IChannel? _channel;

        private static readonly JsonSerializerOptions JsonOptions = new();

        public HeartbeatService(
            IConfiguration configuration,
            ILogger<HeartbeatService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        #region Logger Messages

        [LoggerMessage(
            EventId = 1,
            Level = LogLevel.Information,
            Message = "Heartbeat Service Started.")]
        private static partial void LogServiceStarted(
            ILogger logger);

        [LoggerMessage(
            EventId = 2,
            Level = LogLevel.Information,
            Message = "Heartbeat sent at {Time}")]
        private static partial void LogHeartbeatSent(
            ILogger logger,
            DateTime time);

        [LoggerMessage(
            EventId = 3,
            Level = LogLevel.Information,
            Message = "Stopping Heartbeat Service...")]
        private static partial void LogServiceStopping(
            ILogger logger);

        [LoggerMessage(
            EventId = 4,
            Level = LogLevel.Error,
            Message = "Error while publishing heartbeat.")]
        private static partial void LogHeartbeatError(
            ILogger logger,
            Exception exception);

        #endregion

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            var rabbitConfig = _configuration.GetSection("RabbitMQ");

            var factory = new ConnectionFactory
            {
                HostName = rabbitConfig["HostName"]!,
                Port = int.Parse(rabbitConfig["Port"]!),
                UserName = rabbitConfig["UserName"]!,
                Password = rabbitConfig["Password"]!,
                VirtualHost = rabbitConfig["VirtualHost"]!
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

            LogServiceStarted(_logger);

            await base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            if (_channel == null)
                return;

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var heartbeat = new HeartbeatEvent
                    {
                        ServiceName = "HealthAxis.API",
                        Status = "Running",
                        Timestamp = DateTime.UtcNow,
                        MachineName = Environment.MachineName
                    };

                    var json = JsonSerializer.Serialize(
                        heartbeat,
                        JsonOptions);

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

                    LogHeartbeatSent(
                        _logger,
                        heartbeat.Timestamp);

                    await Task.Delay(
                        TimeSpan.FromSeconds(10),
                        stoppingToken);
                }
                catch (Exception ex)
                {
                    LogHeartbeatError(_logger, ex);

                    await Task.Delay(
                        TimeSpan.FromSeconds(10),
                        stoppingToken);
                }
            }
        }

        public override async Task StopAsync(
            CancellationToken cancellationToken)
        {
            LogServiceStopping(_logger);

            if (_channel != null)
                await _channel.CloseAsync(cancellationToken);

            if (_connection != null)
                await _connection.CloseAsync(cancellationToken);

            await base.StopAsync(cancellationToken);
        }
    }
}