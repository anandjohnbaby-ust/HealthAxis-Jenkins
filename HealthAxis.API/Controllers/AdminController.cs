using HealthAxis.API.DTOs.DoctorDtos;
using HealthAxis.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HealthAxis.API.Controllers
{
    [Route("api/admin")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(
            IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpGet("doctors")]
        public async Task<IActionResult> GetDoctors()
        {
            var doctors =
                await _adminService.GetDoctors();

            return Ok(doctors);
        }

        [HttpPost("doctors")]
        public async Task<IActionResult> CreateDoctor(
            CreateDoctorDto dto)
        {
            var doctor =
                await _adminService.CreateDoctor(dto);

            return Ok(doctor);
        }

        [HttpPut("doctors/{id}")]
        public async Task<IActionResult> UpdateDoctor(
            int id,
            UpdateDoctorDto dto)
        {
            var doctor =
                await _adminService.UpdateDoctor(id, dto);

            return Ok(doctor);
        }

        [HttpGet("reports/appointments")]
        public async Task<IActionResult>
            GetAppointmentReport()
        {
            var report =
                await _adminService.GetAppointmentReport();

            return Ok(report);
        }
    }
}