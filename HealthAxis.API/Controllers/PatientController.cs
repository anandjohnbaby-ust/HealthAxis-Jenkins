using HealthAxis.Shared.DTOs.HealthRecordDtos;
using HealthAxis.Shared.DTOs.PatientDtos;
using HealthAxis.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthAxis.API.Controllers
{
    [Route("api/patients")]
    [ApiController]
    [Authorize(Roles = "Admin,Patient")]
    public class PatientsController : ControllerBase
    {
        private readonly IPatientService _patientService;

        public PatientsController(
            IPatientService patientService)
        {
            _patientService = patientService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PatientDto>>> GetPatients(
            [FromQuery] string? search,
            CancellationToken ct)
        {
            var patients =
                await _patientService.GetPatientsAsync(
                    search,
                    ct);

            return Ok(patients);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<PatientDto>> GetPatientById(
            int id,
            CancellationToken ct)
        {
            var patient =
                await _patientService.GetByIdAsync(
                    id,
                    ct);

            return Ok(patient);
        }

        [HttpPut("{id:int}")]
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
    }
}