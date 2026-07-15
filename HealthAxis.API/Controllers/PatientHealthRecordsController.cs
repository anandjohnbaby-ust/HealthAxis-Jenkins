using HealthAxis.API.Services.Interfaces;
using HealthAxis.Shared.Common;
using HealthAxis.Shared.DTOs.DoctorDtos;
using HealthAxis.Shared.DTOs.HealthRecordDtos;
using HealthAxis.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthAxis.API.Controllers
{
    [Route("api/patients")]
    [ApiController]
    [Authorize(Roles = "Patient")]
    public class PatientHealthRecordsController : ControllerBase
    {
        private readonly IDoctorService _doctorService;

        public PatientHealthRecordsController(
            IDoctorService doctorService)
        {
            _doctorService = doctorService;
        }


        [HttpGet("available-doctors")]
        public async Task<ActionResult<PagedResult<DoctorDto>>> GetAvailableDoctors(
            [FromQuery] Specialisation? specialisation,
            [FromQuery] string? search,
            [FromQuery] PaginationRequest request,
            CancellationToken ct)
        {
            var doctors =
                await _doctorService.GetAvailableDoctorsAsync(
                    specialisation,
                    search,
                    request,
                    ct);

            return Ok(doctors);
        }
    }
}