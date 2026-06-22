using HealthAxis.Shared.DTOs.AuthDtos;

namespace HealthAxis.API.Services.Interfaces
{
    public interface IAuthService
    {
        Task<(bool Success, string Message, string UserId)> Register(RegisterDto request);

        Task<(bool Success, string Message, string AccessToken, string RefreshToken, int ExpiresIn)> Login(LoginDto request);

        Task<(bool Success, string Message, string AccessToken, string RefreshToken, int ExpiresIn)> RefreshToken(RefreshTokenDto request);
    }
}