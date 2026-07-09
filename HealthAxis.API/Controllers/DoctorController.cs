using HealthAxis.API.Services.Implementations;
using HealthAxis.API.Services.Interfaces;
using HealthAxis.Shared.Common;
using HealthAxis.Shared.DTOs.AppointmentDtos;
using HealthAxis.Shared.DTOs.DoctorDtos;
using HealthAxis.Shared.DTOs.HealthRecordDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HealthAxis.API.Controllers
{
    [Route("api/doctors")]
    [ApiController]
    [Authorize]
    public class DoctorsController : ControllerBase
    {
        private readonly IDoctorService _doctorService;
        private readonly IAppointmentService _appointmentService;
        private readonly IHealthRecordService _healthRecordService;

        public DoctorsController(
            IDoctorService doctorService,
            IAppointmentService appointmentService,
            IHealthRecordService healthRecordService)
        {
            _doctorService = doctorService;
            _appointmentService = appointmentService;
            _healthRecordService = healthRecordService;
        }

        //[AllowAnonymous]
        //[HttpGet("{id:int}")]
        //public async Task<IActionResult> GetDoctor(int id)
        //{
        //    return Ok(await _doctorService.GetDoctorById(id));
        //}

        [Authorize(Roles = "Doctor")]
        [HttpGet("me")]
        public async Task<ActionResult<DoctorDto>> GetMyProfile(
            CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var doctor = await _doctorService.GetDoctorByUserIdAsync(
                userId,
                ct);

            return Ok(doctor);
        }

        [Authorize(Roles = "Doctor")]
        [HttpPut("me")]
        public async Task<ActionResult<DoctorDto>> UpdateMyProfile(
            [FromBody] UpdateDoctorDto dto,
            CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var doctor = await _doctorService.UpdateDoctorByUserIdAsync(
                userId,
                dto,
                ct);

            return Ok(doctor);
        }


        [Authorize(Roles = "Doctor")]
        [HttpGet("appointments")]
        public async Task<IActionResult> GetAppointments(CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            return Ok(await _doctorService.GetAppointmentsAsync(userId, ct));
        }

        [Authorize(Roles = "Doctor")]
        [HttpGet("schedule/today")]
        public async Task<IActionResult> GetTodaySchedule(CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            return Ok(await _doctorService.GetTodaysAppointmentsAsync(userId, ct)); 
        }

        [Authorize(Roles = "Doctor")]
        [HttpGet("schedule/week")]
        public async Task<IActionResult> GetWeekSchedule(CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            return Ok(await _doctorService.GetWeeklyAppointmentsAsync(userId, ct));
        }


        [HttpPut("appointments/{appointmentId:int}/status")]
        public async Task<ActionResult<AppointmentDto>> UpdateAppointmentStatus(
            int appointmentId,
            [FromBody] UpdateAppointmentStatusDto dto,
            CancellationToken ct)
        {
            var appointment = await _appointmentService.UpdateStatusAsync(
                appointmentId,
                dto,
                ct);

            return Ok(appointment);
        }

        [HttpPost("health-records")]
        public async Task<ActionResult<HealthRecordDto>> AddHealthRecord(
            CreateHealthRecordDto dto,
            CancellationToken ct)
        {
            var record = await _healthRecordService.AddAsync(
                dto,
                ct);

            return Ok(record);
        }

        [HttpGet("health-records/{id:int}")]
        public async Task<ActionResult<HealthRecordDto>> GetHealthRecord(
            int id,
            CancellationToken ct)
        {
            var healthRecord = await _healthRecordService.GetByRecordIdAsync(
                id,
                ct);

            return Ok(healthRecord);
        }

        [HttpGet("dashboard")]
        public async Task<ActionResult<DoctorDashboardDto>> GetDashboard(
            CancellationToken ct)
        {
            var doctorId = int.Parse(User.FindFirst("doctorId")!.Value);

            var dashboard = await _doctorService.GetDashboardAsync(
                doctorId,
                ct);

            return Ok(dashboard);
        }

        [HttpGet("patient/{patientId:int}/appointment/{appointmentId:int}")]
        [Authorize(Roles = "Doctor")]
        public async Task<ActionResult<IEnumerable<HealthRecordDto>>> GetPatientHealthHistory(
            int patientId,
            int appointmentId,
            CancellationToken ct)
        {
            var records = await _doctorService
                .GetPatientHealthHistoryAsync(
                    patientId,
                    appointmentId,
                    ct);

            return Ok(records);
        }
    }
}