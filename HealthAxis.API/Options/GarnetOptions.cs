namespace HealthAxis.API.Options
{
    public class GarnetOptions
    {
        public string ConnectionString { get; set; } = "localhost:6379";
        public string InstanceName { get; set; } = "HealthAxis:";
    }
}
