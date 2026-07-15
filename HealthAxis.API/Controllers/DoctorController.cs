using HealthAxis.API.Services.Interfaces;
using HealthAxis.Shared.Common;
using HealthAxis.Shared.DTOs.AppointmentDtos;
using HealthAxis.Shared.DTOs.DoctorDtos;
using HealthAxis.Shared.DTOs.HealthRecordDtos;
using HealthAxis.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HealthAxis.API.Controllers
{
    [Route("api/doctors")]
    [ApiController]
    [Authorize(Roles = "Doctor")]
    public class DoctorsController : ControllerBase
    {
        private readonly IDoctorService _doctorService;

        public DoctorsController(IDoctorService doctorService)
        {
            _doctorService = doctorService;
        }

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

        [HttpGet("appointments")]
        public async Task<ActionResult<PagedResult<AppointmentDto>>> GetAppointments(
            [FromQuery] PaginationRequest request,
            [FromQuery] string? search,
            [FromQuery] AppointmentStatus? status,
            [FromQuery] DateTime? date,
            CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var appointments = await _doctorService.GetAppointmentsAsync(
                userId,
                request,
                search,
                status,
                date,
                ct);

            return Ok(appointments);
        }

        [HttpGet("schedule/today")]
        public async Task<ActionResult<PagedResult<AppointmentDto>>> GetTodaySchedule(
            [FromQuery] PaginationRequest request,
            [FromQuery] string? search,
            [FromQuery] AppointmentStatus? status,
            CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var appointments = await _doctorService.GetTodaysAppointmentsAsync(
                userId,
                request,
                search,
                status,
                ct);

            return Ok(appointments);
        }

        [HttpGet("schedule/week")]
        public async Task<ActionResult<PagedResult<AppointmentDto>>> GetWeekSchedule(
            [FromQuery] PaginationRequest request,
            [FromQuery] string? search,
            [FromQuery] AppointmentStatus? status,
            [FromQuery] DateTime? date,
            CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var appointments = await _doctorService.GetWeeklyAppointmentsAsync(
                userId,
                request,
                search,
                status,
                date,
                ct);

            return Ok(appointments);
        }

        [HttpPut("appointments/{appointmentId:int}/status")]
        public async Task<ActionResult<AppointmentDto>> UpdateAppointmentStatus(
            int appointmentId,
            [FromBody] UpdateAppointmentStatusDto dto,
            CancellationToken ct)
        {
            var appointment = await _doctorService.UpdateStatusAsync(
                appointmentId,
                dto,
                ct);

            return Ok(appointment);
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

        [HttpPost("health-records")]
        public async Task<ActionResult<HealthRecordDto>> AddHealthRecord(
            [FromBody] CreateHealthRecordDto dto,
            CancellationToken ct)
        {
            var record = await _doctorService.AddAsync(
                dto,
                ct);

            return Ok(record);
        }

        [HttpGet("patient/{patientId:int}/appointment/{appointmentId:int}")]
        public async Task<ActionResult<IEnumerable<HealthRecordDto>>> GetPatientHealthHistory(
            int patientId,
            int appointmentId,
            CancellationToken ct)
        {
            var records = await _doctorService.GetPatientHealthHistoryAsync(
                patientId,
                appointmentId,
                ct);

            return Ok(records);
        }
    }
}