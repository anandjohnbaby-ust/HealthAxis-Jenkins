using HealthAxis.API.Services.Interfaces;
using HealthAxis.Shared.Common;
using HealthAxis.Shared.DTOs.AdminDtos;
using HealthAxis.Shared.DTOs.DoctorDtos;
using HealthAxis.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthAxis.API.Controllers
{
    [Route("api/admin/doctors")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminDoctorsController : ControllerBase
    {
        private readonly IDoctorService _doctorService;

        public AdminDoctorsController(IDoctorService doctorService)
        {
            _doctorService = doctorService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<DoctorDto>>> GetDoctors(
            [FromQuery] PaginationRequest request,
            [FromQuery] Specialisation? specialisation,
            [FromQuery] string? search,
            [FromQuery] bool? isActive,
            CancellationToken ct)
        {
            var doctors = await _doctorService.GetDoctorsAsync(
                request,
                specialisation,
                search,
                isActive,
                ct);

            return Ok(doctors);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetDoctorById(int id)
        {
            var doctor = await _doctorService.GetDoctorById(id);

            return Ok(doctor);
        }

        [HttpPost]
        public async Task<IActionResult> CreateDoctor(CreateDoctorDto dto)
        {
            var doctor = await _doctorService.CreateDoctor(dto);

            return CreatedAtAction(
                nameof(GetDoctorById),
                new { id = doctor.DoctorId },
                doctor);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateDoctor(
            int id,
            UpdateDoctorDto dto)
        {
            var doctor = await _doctorService.UpdateDoctor(id, dto);

            return Ok(doctor);
        }
    }
}