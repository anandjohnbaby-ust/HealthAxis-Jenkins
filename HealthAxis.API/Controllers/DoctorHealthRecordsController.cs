using HealthAxis.API.Services.Interfaces;
using HealthAxis.Shared.DTOs.HealthRecordDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthAxis.API.Controllers
{
    [Route("api/doctors")]
    [ApiController]
    [Authorize(Roles = "Doctor")]
    public class DoctorHealthRecordsController : ControllerBase
    {
        private readonly IHealthRecordService _healthRecordService;

        public DoctorHealthRecordsController(
            IHealthRecordService healthRecordService)
        {
            _healthRecordService = healthRecordService;
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

    }
}