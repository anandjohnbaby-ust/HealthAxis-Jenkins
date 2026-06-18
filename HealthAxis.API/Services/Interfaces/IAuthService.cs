using HealthAxis.API.DTOs.AuthDtos;

namespace HealthAxis.API.Services.Interfaces
{
    public interface IAuthService
    {
        Task<(bool Success, string Message, string UserId)> Register(RegisterDto request);
        Task<(bool Success, string Message, string token, int ExpiresIn)> Login(LoginDto request);


    }
}

