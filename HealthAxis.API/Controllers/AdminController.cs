using HealthAxis.API.Services.Interfaces;
using HealthAxis.Shared.Common;
using HealthAxis.Shared.DTOs.AdminDtos;
using HealthAxis.Shared.DTOs.AuthDtos;
using HealthAxis.Shared.DTOs.DoctorDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthAxis.API.Controllers
{
    [Route("api/admin")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpGet("doctors")]
        public async Task<IActionResult> GetDoctors()
        {
            var doctors = await _adminService.GetDoctors();

            return Ok(doctors);
        }

        [HttpPost("doctors")]
        public async Task<IActionResult> CreateDoctor(
            Shared.DTOs.AdminDtos.CreateDoctorDto dto)
        {
            var doctor = await _adminService.CreateDoctor(dto);

            return CreatedAtAction(
                nameof(GetDoctors),
                new { id = doctor.DoctorId },
                doctor);
        }

        [HttpPut("doctors/{id:int}")]
        public async Task<IActionResult> UpdateDoctor(
            int id,
            UpdateDoctorDto dto)
        {
            var doctor = await _adminService.UpdateDoctor(id, dto);

            return Ok(doctor);
        }

        [HttpGet("reports/appointments")]
        public async Task<IActionResult> GetAppointmentReport()
        {
            var report = await _adminService.GetAppointmentReport();

            return Ok(report);
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers(
            [FromQuery] string? role,
            [FromQuery] PaginationRequest request)
        {
            var users = await _adminService.GetUsers(role, request);

            return Ok(users);
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var dashboard = await _adminService.GetDashboardAsync();

            return Ok(dashboard);
        }
    }
}