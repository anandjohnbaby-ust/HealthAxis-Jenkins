namespace HealthAxis.API.Events;

public class HeartbeatEvent
{
    public string ServiceName { get; set; } = string.Empty;

    public string Status { get; set; } = "Running";

    public DateTime Timestamp { get; set; }

    public string MachineName { get; set; } = Environment.MachineName;
}