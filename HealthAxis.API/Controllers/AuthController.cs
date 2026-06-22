using HealthAxis.Shared.DTOs.AuthDtos;
using HealthAxis.API.Models;
using HealthAxis.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HealthAxis.API.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService service) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto request)
        {
            var (success, message, userId) =
                await service.Register(request);

            if (!success)
            {
                return BadRequest(new
                {
                    message
                });
            }

            return Ok(new
            {
                message,
                userId
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto request)
        {
            var result = await service.Login(request);

            if (!result.Success)
            {
                return Unauthorized(new
                {
                    result.Message
                });
            }

            return Ok(new AuthResponse
            {
                Message = result.Message,
                AccessToken = result.AccessToken,
                RefreshToken = result.RefreshToken,
                ExpiresIn = result.ExpiresIn
            });
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken(
            RefreshTokenDto request)
        {
            var result = await service.RefreshToken(request);

            if (!result.Success)
            {
                return Unauthorized(new
                {
                    result.Message
                });
            }

            return Ok(new AuthResponse
            {
                Message = result.Message,
                AccessToken = result.AccessToken,
                RefreshToken = result.RefreshToken,
                ExpiresIn = result.ExpiresIn
            });
        }
    }
}