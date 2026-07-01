using HealthAxis.API.Services.Implementations;
using HealthAxis.API.Services.Interfaces;
using HealthAxis.Shared.Common;
using HealthAxis.Shared.DTOs.AppointmentDtos;
using HealthAxis.Shared.DTOs.DoctorDtos;
using HealthAxis.Shared.DTOs.HealthRecordDtos;
using HealthAxis.Shared.DTOs.PatientDtos;
using HealthAxis.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HealthAxis.API.Controllers
{
    [Route("api/patients")]
    [ApiController]
    [Authorize(Roles = "Patient")]
    public class PatientsController : ControllerBase
    {
        #region Dependency Injection
        private readonly IPatientService _patientService;

        public PatientsController(
            IPatientService patientService)
        {
            _patientService = patientService;
        }

        #endregion

        [HttpGet("me")]
        public async Task<ActionResult<PatientDto>> GetMyProfile(
            CancellationToken ct)
        {
            var patientId = int.Parse(User.FindFirst("patientId")!.Value);

            var patient = await _patientService.GetByIdAsync(patientId, ct);

            return Ok(patient);
        }

        [HttpGet("{id:int}/health-records")]
        public async Task<ActionResult<IEnumerable<HealthRecordDto>>> GetHealthRecords(
            int id,
            CancellationToken ct)
        {
            var records =
                await _patientService.GetHealthRecordsByPatientId(
                    id,
                    ct);

            return Ok(records);
        }

        [HttpPost("{id:int}/book-appointments")]
        public async Task<ActionResult<AppointmentDto>> BookAppointment(
            int id,
            CreateAppointmentDto dto,
            CancellationToken ct)
        {
            dto.PatientId = id;

            var appointment = await _patientService.BookAppointmentAsync(dto, ct);

            return CreatedAtAction(
                nameof(BookAppointment),
                new { id = appointment.AppointmentId },
                appointment);
        }


        [HttpGet("available-doctors")]
        public async Task<ActionResult<IEnumerable<DoctorDto>>> GetAvailableDoctors(
            [FromQuery] Specialisation? specialisation,
            [FromQuery] string? search,
            CancellationToken ct)
        {
            var doctors = await _patientService.GetAvailableDoctorsAsync(
                specialisation,
                search,
                ct);

            return Ok(doctors);
        }

        [HttpGet("{id:int}/appointments")]
        public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetAppointments(
            int id,
            CancellationToken ct)
        {
            var appointments =
                await _patientService.GetAppointmentsByPatientIdAsync(
                    id,
                    ct);

            return Ok(appointments);
        }

        [HttpPut("{patientId:int}/appointments/{appointmentId:int}/cancel")]
        public async Task<ActionResult<AppointmentDto>> CancelAppointment(
            int patientId,
            int appointmentId,
            CancelAppointmentDto dto,
            CancellationToken ct)
        {
            var appointment =
                await _patientService.CancelAppointmentByPatientAsync(
                    patientId,
                    appointmentId,
                    dto,
                    ct);

            return Ok(appointment);
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


    }
}