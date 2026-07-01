using HealthAxis.Shared.DTOs.AuthDtos;
using HealthAxis.Shared.DTOs.CommonDtos;

namespace HealthAxis.API.Services.Interfaces
{
    public interface IAuthService
    {
        Task<(bool Success, string Message, string UserId)> Register(RegisterDto request);

        Task<(bool Success, string Message, string AccessToken, string RefreshToken, int ExpiresIn)> Login(LoginDto request);

        Task<(bool Success, string Message, string AccessToken, string RefreshToken, int ExpiresIn)> RefreshToken(RefreshTokenDto request);

        Task ChangePasswordAsync(string userId, ChangePasswordDto dto);
    }
}