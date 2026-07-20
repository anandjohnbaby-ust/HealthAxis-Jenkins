namespace HealthAxis.API.BackgroundServices;

public partial class HeartbeatService : BackgroundService
{
    private readonly ILogger<HeartbeatService> _logger;

    public HeartbeatService(
        ILogger<HeartbeatService> logger)
    {
        _logger = logger;
    }

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "HealthAxis API started.")]
    private static partial void LogStarted(
        ILogger logger);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Information,
        Message = "Heartbeat - API is alive at {Time}.")]
    private static partial void LogHeartbeat(
        ILogger logger,
        DateTime time);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Information,
        Message = "HealthAxis API stopped.")]
    private static partial void LogStopped(
        ILogger logger);

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        LogStarted(_logger);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                LogHeartbeat(_logger, DateTime.UtcNow);

                await Task.Delay(
                    TimeSpan.FromSeconds(10),
                    stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Application is shutting down.
        }

        LogStopped(_logger);
    }
}