using HealthAxis.API.DTOs.AppointmentDtos;
using HealthAxis.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HealthAxis.API.Controllers
{
    [Route("api/appointments")]
    [ApiController]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;

        public AppointmentsController(
            IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AppointmentDto>>>
            GetAllAppointments(
                CancellationToken ct)
        {
            var appointments =
                await _appointmentService.GetAllAsync(ct);

            return Ok(appointments);
        }

        [HttpPost]
        public async Task<ActionResult<AppointmentDto>>
            CreateAppointment(
                CreateAppointmentDto dto,
                CancellationToken ct)
        {
            var createdAppointment =
                await _appointmentService.AddAsync(
                    dto,
                    ct);

            return CreatedAtAction(
                nameof(GetAllAppointments),
                new { id = createdAppointment.AppointmentId },
                createdAppointment);
        }

        [HttpPut("{id:int}/status")]
        public async Task<ActionResult<AppointmentDto>>
            UpdateAppointmentStatus(
                int id,
                UpdateAppointmentStatusDto dto,
                CancellationToken ct)
        {
            try
            {
                var updatedAppointment =
                    await _appointmentService
                        .UpdateStatusAsync(
                            id,
                            dto,
                            ct);

                return Ok(updatedAppointment);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<AppointmentDto>>
            DeleteAppointment(
                int id,
                CancellationToken ct)
        {
            try
            {
                var deletedAppointment =
                    await _appointmentService
                        .DeleteAsync(
                            id,
                            ct);

                return Ok(deletedAppointment);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}