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