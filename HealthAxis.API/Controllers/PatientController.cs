using HealthAxis.API.Services.Interfaces;
using HealthAxis.Shared.Common;
using HealthAxis.Shared.DTOs.AppointmentDtos;
using HealthAxis.Shared.DTOs.DoctorDtos;
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
        #region Dependency Injection

        private readonly IPatientService _patientService;
        private readonly IDoctorService _doctorService;
        private readonly IAppointmentService _appointmentService;

        public PatientsController(
            IPatientService patientService,
            IDoctorService doctorService,
            IAppointmentService appointmentService)
        {
            _patientService = patientService;
            _doctorService = doctorService;
            _appointmentService = appointmentService;
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

        [HttpPost("{id:int}/book-appointments")]
        public async Task<ActionResult<AppointmentDto>> BookAppointment(
            int id,
            CreateAppointmentDto dto,
            CancellationToken ct)
        {
            dto.PatientId = id;

            var appointment = await _appointmentService.BookAppointmentAsync(dto, ct);

            return CreatedAtAction(
                nameof(BookAppointment),
                new { id = appointment.AppointmentId },
                appointment);
        }
        
        [HttpGet("available-doctors")]
        public async Task<ActionResult<PagedResult<DoctorDto>>> GetAvailableDoctors(
            [FromQuery] Specialisation? specialisation,
            [FromQuery] string? search,
            [FromQuery] PaginationRequest request,
            CancellationToken ct)
        {
            var doctors = await _doctorService.GetAvailableDoctorsAsync(
                specialisation,
                search,
                request,
                ct);

            return Ok(doctors);
        }

        [HttpGet("{id:int}/appointments")]
        public async Task<ActionResult<PagedResult<AppointmentDto>>> GetAppointments(
            int id,
            [FromQuery] PaginationRequest request,
            CancellationToken ct)
        {
            var appointments =
                await _patientService.GetAppointmentsByPatientIdAsync(
                    id,
                    request,
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
                await _appointmentService.CancelAppointmentByPatientAsync(
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

        [HttpGet("available-slots")]
        public async Task<IActionResult> GetAvailableSlots(
            int doctorId,
            DateTime date,
            CancellationToken ct)
        {
            var slots = await _appointmentService
                .GetAvailableSlotsAsync(doctorId, date, ct);

            return Ok(slots);
        }

    }
}