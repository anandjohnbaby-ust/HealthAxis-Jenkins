using HealthAxis.Shared.DTOs.HealthRecordDtos;
using HealthAxis.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthAxis.API.Controllers
{
    [Route("api/health-records")]
    [ApiController]
    [Authorize]
    public class HealthRecordsController : ControllerBase
    {
        private readonly IHealthRecordService _healthRecordService;

        public HealthRecordsController(
            IHealthRecordService healthRecordService)
        {
            _healthRecordService = healthRecordService;
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<IActionResult> GetByRecordIdAsync(
            int id,
            CancellationToken ct)
        {
            var record = await _healthRecordService
                .GetByRecordIdAsync(id, ct);

            return Ok(record);
        }

        [HttpPost]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> Create(
            CreateHealthRecordDto dto,
            CancellationToken ct)
        {
            var healthRecord = await _healthRecordService
                .AddAsync(dto, ct);

            return Ok(healthRecord);
        }
    }
}