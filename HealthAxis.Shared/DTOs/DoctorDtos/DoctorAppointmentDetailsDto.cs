using HealthAxis.Shared.DTOs.AppointmentDtos;
using HealthAxis.Shared.DTOs.HealthRecordDtos;
using HealthAxis.Shared.DTOs.PatientDtos;

public class DoctorAppointmentDetailsDto
{
    public AppointmentDto Appointment { get; set; } = default!;

    public PatientDto Patient { get; set; } = default!;

    public List<HealthRecordDto> PreviousHealthRecords { get; set; } = [];
}