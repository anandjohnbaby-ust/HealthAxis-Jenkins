using HealthAxis.Shared.DTOs.AppointmentDtos;
using HealthAxis.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthAxis.API.Controllers
{
    [Route("api/appointments")]
    [ApiController]
    [Authorize]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;

        public AppointmentsController(
            IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        [HttpGet]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<ActionResult<IEnumerable<AppointmentDto>>>
            GetAllAppointments(CancellationToken ct)
        {
            var appointments =
                await _appointmentService.GetAllAsync(ct);

            return Ok(appointments);
        }

        [HttpPost]
        [Authorize(Roles = "Patient")]
        public async Task<ActionResult<AppointmentDto>>
            CreateAppointment(
                CreateAppointmentDto dto,
                CancellationToken ct)
        {
            var createdAppointment =
                await _appointmentService.AddAsync(dto, ct);

            return CreatedAtAction(
                nameof(GetAllAppointments),
                new { id = createdAppointment.AppointmentId },
                createdAppointment);
        }

        [HttpPut("{id:int}/status")]
        [Authorize(Roles = "Doctor")]
        public async Task<ActionResult<AppointmentDto>>
            UpdateAppointmentStatus(
                int id,
                UpdateAppointmentStatusDto dto,
                CancellationToken ct)
        {
            var updatedAppointment =
                await _appointmentService.UpdateStatusAsync(
                    id,
                    dto,
                    ct);

            return Ok(updatedAppointment);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<AppointmentDto>>
            DeleteAppointment(
                int id,
                CancellationToken ct)
        {
            var deletedAppointment =
                await _appointmentService.DeleteAsync(
                    id,
                    ct);

            return Ok(deletedAppointment);
        }
    }
}