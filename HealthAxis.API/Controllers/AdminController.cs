using HealthAxis.API.Repositories.Implementations;
using HealthAxis.API.Repositories.Interfaces;
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

        #region Dependency injection
        private readonly IAdminService _adminService;
        private readonly IDoctorService _doctorService;
        private readonly IPatientService _patientService;
        private readonly IAppointmentService _appointmentService;

        public AdminController(IAdminService adminService,
            IDoctorService doctorService,
            IPatientService patientService,
            IAppointmentService appointmentService)
        {
            _adminService = adminService;
            _doctorService = doctorService;
            _patientService = patientService;
            _appointmentService = appointmentService;
        }
        #endregion

        #region Doctors
        [HttpGet("doctors")]
        public async Task<ActionResult<PagedResult<DoctorDto>>> GetDoctors(
            [FromQuery] PaginationRequest request,
            [FromQuery] Specialisation? specialisation,
            [FromQuery] string? search,
            [FromQuery] bool? isActive,
            CancellationToken ct)
        {
            var doctors = await _doctorService.GetDoctorsAsync(
                request,
                specialisation,
                search,
                isActive,
                ct);

            return Ok(doctors);
        }

        [HttpGet("doctors/{id:int}")]
        public async Task<IActionResult> GetDoctorById(int id)
        {
            var doctor = await _doctorService.GetDoctorById(id);

            return Ok(doctor);
        }

        [HttpPost("doctors")]
        public async Task<IActionResult> CreateDoctor(
            CreateDoctorDto dto)
        {
            var doctor = await _doctorService.CreateDoctor(dto);

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
            var doctor = await _doctorService.UpdateDoctor(id, dto);

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
            var patients = await _patientService.GetPatientsAsync(
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
                await _patientService.UpdateAsync(
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
            var report = await _appointmentService.GetAppointmentReportAsync(request);

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