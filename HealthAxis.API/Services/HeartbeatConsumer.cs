using System.Text;
using System.Text.Json;
using HealthAxis.API.Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace HealthAxis.API.Services;

public class HeartbeatConsumer : BackgroundService
{
    private readonly ILogger<HeartbeatConsumer> _logger;
    private readonly IConfiguration _configuration;

    private IConnection? _connection;
    private IChannel? _channel;

    public HeartbeatConsumer(
        ILogger<HeartbeatConsumer> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
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

        _logger.LogInformation("Heartbeat Consumer Started.");

        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        if (_channel == null)
            return;

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (sender, eventArgs) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(
                    eventArgs.Body.ToArray());

                var heartbeat = JsonSerializer.Deserialize<HeartbeatEvent>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                if (heartbeat != null)
                {
                    _logger.LogInformation(
                        "HEARTBEAT RECEIVED | Service: {Service} | Status: {Status} | Machine: {Machine} | Time: {Time}",
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
                _logger.LogError(
                    ex,
                    "Error processing heartbeat.");

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
        _logger.LogInformation("Stopping Heartbeat Consumer...");

        if (_channel != null)
            await _channel.CloseAsync(cancellationToken);

        if (_connection != null)
            await _connection.CloseAsync(cancellationToken);

        await base.StopAsync(cancellationToken);
    }
}