using HealthAxis.API.DTOs.HealthRecordDtos;
using HealthAxis.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

    [Route("api/health-records")]
    [ApiController]
    public class HealthRecordsController :
    ControllerBase
{
    private readonly IHealthRecordService
        _healthRecordService;

    public HealthRecordsController(
        IHealthRecordService healthRecordService)
    {
        _healthRecordService =
            healthRecordService;
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult>
        GetByRecordIdAsync(
            int id,
            CancellationToken ct)
    {
        var record =
            await _healthRecordService
                .GetByRecordIdAsync(
                    id,
                    ct);

        return Ok(record);
    }

    [HttpPost]
    public async Task<IActionResult>
        Create(
            CreateHealthRecordDto dto,
            CancellationToken ct)
    {
        var healthRecord =
            await _healthRecordService
                .AddAsync(
                    dto,
                    ct);

        return Ok(healthRecord);
    }

    [HttpGet("patient/{patientId:int}")]
    public async Task<ActionResult<IEnumerable<HealthRecordDto>>> GetByPatientId(
        int patientId,
        CancellationToken ct)
    {
        var records =
            await _healthRecordService.GetByPatientIdAsync(
                patientId,
                ct);

        return Ok(records);
    }
}