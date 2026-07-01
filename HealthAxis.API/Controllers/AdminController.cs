using HealthAxis.API.Services.Implementations;
using HealthAxis.API.Services.Interfaces;
using HealthAxis.Shared.Common;
using HealthAxis.Shared.DTOs.AdminDtos;
using HealthAxis.Shared.DTOs.AuthDtos;
using HealthAxis.Shared.DTOs.DoctorDtos;
using HealthAxis.Shared.DTOs.PatientDtos;
using HealthAxis.Shared.Enums;
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

        #region Doctors

        [HttpGet("doctors")]
        public async Task<ActionResult<PagedResult<DoctorDto>>> GetDoctors(
            [FromQuery] PaginationRequest request,
            [FromQuery] Specialisation? specialisation,
            [FromQuery] string? search,
            CancellationToken ct)
        {
            var doctors = await _adminService.GetDoctorsAsync(
                request,
                specialisation,
                search,
                ct);

            return Ok(doctors);
        }

        [HttpGet("doctors/{id:int}")]
        public async Task<IActionResult> GetDoctorById(int id)
        {
            var doctor = await _adminService.GetDoctorById(id);

            return Ok(doctor);
        }

        [HttpPost("doctors")]
        public async Task<IActionResult> CreateDoctor(
            Shared.DTOs.AdminDtos.CreateDoctorDto dto)
        {
            var doctor = await _adminService.CreateDoctor(dto);

            return CreatedAtAction(
                nameof(GetDoctorById),
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

        #endregion

        #region Patients

        [HttpGet("patients")]
        public async Task<ActionResult<PagedResult<PatientDto>>> GetPatients(
            [FromQuery] PaginationRequest request,
            [FromQuery] string? search,
            CancellationToken ct)
        {
            var patients = await _adminService.GetPatientsAsync(
                request,
                search,
                ct);

            return Ok(patients);
        }

        [HttpPut("patients/{id:int}")]
        public async Task<ActionResult<PatientDto>> UpdatePatient(
            int id,
            UpdatePatientDto dto,
            CancellationToken ct)
        {
            var updatedPatient =
                await _adminService.UpdatePatientAsync(
                    id,
                    dto,
                    ct);

            return Ok(updatedPatient);
        }

        #endregion

        #region Appointment

        [HttpGet("reports/appointments")]
        public async Task<ActionResult<PagedResult<AppointmentReportDto>>> GetAppointmentReport(
            [FromQuery] PaginationRequest request)
        {
            var report = await _adminService.GetAppointmentReport(request);

            return Ok(report);
        }
        #endregion

        #region DashBoard
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var dashboard = await _adminService.GetDashboardAsync();

            return Ok(dashboard);
        }
        #endregion


    }
}