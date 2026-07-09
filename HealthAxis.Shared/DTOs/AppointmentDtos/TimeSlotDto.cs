namespace HealthAxis.Shared.DTOs.AppointmentDtos
{
    public class TimeSlotDto
    {
        public string Value { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;

        public bool IsAvailable { get; set; } = true;
    }
}