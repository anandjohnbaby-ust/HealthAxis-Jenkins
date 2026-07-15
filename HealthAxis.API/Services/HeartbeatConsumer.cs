using System.Text;
using System.Text.Json;
using HealthAxis.API.Events;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace HealthAxis.API.Services;

public partial class HeartbeatConsumer : BackgroundService
{
    private readonly ILogger<HeartbeatConsumer> _logger;
    private readonly IConfiguration _configuration;

    private IConnection? _connection;
    private IChannel? _channel;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public HeartbeatConsumer(
        ILogger<HeartbeatConsumer> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    #region Logger Messages

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Heartbeat Consumer Started.")]
    private static partial void LogConsumerStarted(
        ILogger logger);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Information,
        Message = "HEARTBEAT RECEIVED | Service: {Service} | Status: {Status} | Machine: {Machine} | Time: {Time}")]
    private static partial void LogHeartbeatReceived(
        ILogger logger,
        string service,
        string status,
        string machine,
        DateTime time);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Error,
        Message = "Error processing heartbeat.")]
    private static partial void LogHeartbeatError(
        ILogger logger,
        Exception exception);

    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Information,
        Message = "Stopping Heartbeat Consumer...")]
    private static partial void LogConsumerStopping(
        ILogger logger);

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

        LogConsumerStarted(_logger);

        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        if (_channel == null)
            return;

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (_, eventArgs) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(eventArgs.Body.ToArray());

                var heartbeat = JsonSerializer.Deserialize<HeartbeatEvent>(
                    json,
                    JsonOptions);

                if (heartbeat != null)
                {
                    LogHeartbeatReceived(
                        _logger,
                        heartbeat.ServiceName,
                        heartbeat.Status,
                        heartbeat.MachineName,
                        heartbeat.Timestamp);
                }

                await _channel.BasicAckAsync(
                    eventArgs.DeliveryTag,
                    false);
            }
            catch (Exception ex)
            {
                LogHeartbeatError(_logger, ex);

                await _channel.BasicNackAsync(
                    eventArgs.DeliveryTag,
                    false,
                    true);
            }
        };

        await _channel.BasicConsumeAsync(
            queue: "heartbeat-queue",
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
        LogConsumerStopping(_logger);

        if (_channel != null)
            await _channel.CloseAsync(cancellationToken);

        if (_connection != null)
            await _connection.CloseAsync(cancellationToken);

        await base.StopAsync(cancellationToken);
    }
}