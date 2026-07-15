using HealthAxis.API.Services.Interfaces;
using HealthAxis.Shared.Common;
using HealthAxis.Shared.DTOs.AppointmentDtos;
using HealthAxis.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthAxis.API.Controllers
{
    [Route("api/patients")]
    [ApiController]
    [Authorize(Roles = "Patient")]
    public class PatientAppointmentsController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;

        public PatientAppointmentsController(
            IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        [HttpPost("{id:int}/book-appointments")]
        public async Task<ActionResult<AppointmentDto>> BookAppointment(
            int id,
            CreateAppointmentDto dto,
            CancellationToken ct)
        {
            dto.PatientId = id;

            var appointment =
                await _appointmentService.BookAppointmentAsync(
                    dto,
                    ct);

            return CreatedAtAction(
                nameof(BookAppointment),
                new { id = appointment.AppointmentId },
                appointment);
        }


        [HttpPut("{patientId:int}/appointments/{appointmentId:int}/cancel")]
        public async Task<ActionResult<AppointmentDto>> CancelAppointment(
            int patientId,
            int appointmentId,
            CancelAppointmentDto dto,
            CancellationToken ct)
        {
            var appointment =
                await _appointmentService.CancelAppointmentByPatientAsync(
                    patientId,
                    appointmentId,
                    dto,
                    ct);

            return Ok(appointment);
        }

        [HttpGet("available-slots")]
        public async Task<IActionResult> GetAvailableSlots(
            int doctorId,
            DateTime date,
            CancellationToken ct)
        {
            var slots = await _appointmentService.GetAvailableSlotsAsync(
                doctorId,
                date,
                ct);

            return Ok(slots);
        }
    }
}