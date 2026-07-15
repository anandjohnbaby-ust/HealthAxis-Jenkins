using HealthAxis.API.Services.Interfaces;
using HealthAxis.Shared.Common;
using HealthAxis.Shared.DTOs.PatientDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthAxis.API.Controllers
{
    [Route("api/admin/patients")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminPatientsController : ControllerBase
    {
        private readonly IPatientService _patientService;

        public AdminPatientsController(IPatientService patientService)
        {
            _patientService = patientService;
        }

        [HttpGet]
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

        [HttpPut("{id:int}")]
        public async Task<ActionResult<PatientDto>> UpdatePatient(
            int id,
            UpdatePatientDto dto,
            CancellationToken ct)
        {
            var patient = await _patientService.UpdateAsync(
                id,
                dto,
                ct);

            return Ok(patient);
        }
    }
}