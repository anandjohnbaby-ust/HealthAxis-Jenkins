using HealthAxis.API.Services.Interfaces;
using HealthAxis.Shared.Common;
using HealthAxis.Shared.DTOs.AppointmentDtos;
using HealthAxis.Shared.DTOs.HealthRecordDtos;
using HealthAxis.Shared.DTOs.PatientDtos;
using HealthAxis.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthAxis.API.Controllers
{
    [Route("api/patients")]
    [ApiController]
    [Authorize(Roles = "Patient")]
    public class PatientsController : ControllerBase
    {
        private readonly IPatientService _patientService;

        public PatientsController(IPatientService patientService)
        {
            _patientService = patientService;
        }

        [HttpGet("me")]
        public async Task<ActionResult<PatientDto>> GetMyProfile(
            CancellationToken ct)
        {
            var patientId = int.Parse(User.FindFirst("patientId")!.Value);

            var patient = await _patientService.GetByIdAsync(
                patientId,
                ct);

            return Ok(patient);
        }

        [HttpPut("me")]
        public async Task<ActionResult<PatientDto>> UpdateMyProfile(
            UpdatePatientDto dto,
            CancellationToken ct)
        {
            var patientId = int.Parse(User.FindFirst("patientId")!.Value);

            var patient = await _patientService.UpdateAsync(
                patientId,
                dto,
                ct);

            return Ok(patient);
        }

        [HttpGet("dashboard")]
        public async Task<ActionResult<PatientDashboardDto>> GetDashboard(
            CancellationToken ct)
        {
            var patientId = int.Parse(User.FindFirst("patientId")!.Value);

            var dashboard = await _patientService.GetDashboardAsync(
                patientId,
                ct);

            return Ok(dashboard);
        }

        [HttpGet("{id:int}/appointments")]
        public async Task<ActionResult<PagedResult<AppointmentDto>>> GetAppointments(
            int id,
            [FromQuery] PaginationRequest request,
            [FromQuery] string? search,
            [FromQuery] AppointmentStatus? status,
            [FromQuery] DateTime? date,
            CancellationToken ct)
        {
            var appointments =
                await _patientService.GetAppointmentsByPatientIdAsync(
                    id,
                    request,
                    search,
                    status,
                    date,
                    ct);

            return Ok(appointments);
        }


        [HttpGet("{id:int}/health-records")]
        public async Task<ActionResult<PagedResult<HealthRecordDto>>> GetHealthRecords(
            int id,
            [FromQuery] PaginationRequest request,
            CancellationToken ct)
        {
            var records =
                await _patientService.GetHealthRecordsByPatientId(
                    id,
                    request,
                    ct);

            return Ok(records);
        }
    }
}