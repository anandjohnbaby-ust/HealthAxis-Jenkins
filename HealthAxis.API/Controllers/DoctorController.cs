using HealthAxis.API.DTOs.DoctorDtos;
using HealthAxis.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthAxis.API.Controllers
{
    [Route("api/doctors")]
    [ApiController]
    [Authorize]
    public class DoctorsController : ControllerBase
    {
        private readonly IDoctorService _doctorService;

        public DoctorsController(
            IDoctorService doctorService)
        {
            _doctorService = doctorService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<DoctorDto>>>
            GetAllDoctors(
                CancellationToken ct)
        {
            var doctors =
                await _doctorService.GetAllAsync(ct);

            return Ok(doctors);
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<ActionResult<DoctorDto>>
            GetDoctorById(
                int id,
                CancellationToken ct)
        {
            var doctor =
                await _doctorService.GetByIdAsync(id, ct);

            return Ok(doctor);
        }

        [HttpGet("{id:int}/availability")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult>
            GetDoctorAvailability(
                int id,
                CancellationToken ct)
        {
            DoctorDto doctor =
                await _doctorService
                    .GetAvailableDoctorByIdAsync(
                        id,
                        ct);

            return Ok(doctor);
        }

        [Authorize]
        [HttpGet("whoami")]
        public IActionResult WhoAmI()
        {
            return Ok(new
            {
                Name = User.Identity?.Name,
                IsAuthenticated = User.Identity?.IsAuthenticated,
                Claims = User.Claims.Select(c => new
                {
                    c.Type,
                    c.Value
                })
            });
        }
    }
}