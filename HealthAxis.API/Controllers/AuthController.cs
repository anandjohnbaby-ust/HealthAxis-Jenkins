using HealthAxis.API.DTOs.AuthDtos;
using HealthAxis.API.Models;
using HealthAxis.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HealthAxis.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService service) : ControllerBase
    {
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto request)
        {

            var (success, message, userId) = await service.Register(request);

            if (!success)
            {
                return BadRequest(new { message });
            }
            return Ok(new { message, userId });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto request)
        {
            var (success, message, token, expiresIn) = await service.Login(request);
            if (!success)
            {
                return Unauthorized(new { message });
            }

            AuthResponse response = new AuthResponse
            {
                AccessToken = token,
                Message = message,
                ExpiresIn = expiresIn
            };

            return Ok(response);


        }
    }


}