using HealthAxis.API.DTOs.AuthDtos;
using HealthAxis.API.Models;
using HealthAxis.API.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HealthAxis.API.Services.Implementation
{
    public class AuthService(UserManager<ApplicationUser> userManager, IConfiguration config) : IAuthService
    {
        public async Task<(bool Success,
                  string Message,
                  string AccessToken,
                  string RefreshToken,
                  int ExpiresIn)>
                    Login(LoginDto request)
        {
            var user = await userManager.FindByEmailAsync(request.Email);

            if (user == null)
            {
                return (false,
                        "Invalid Credentials",
                        string.Empty,
                        string.Empty,
                        0);
            }

            var validPassword =
                await userManager.CheckPasswordAsync(user, request.Password);

            if (!validPassword)
            {
                return (false,
                        "Invalid Credentials",
                        string.Empty,
                        string.Empty,
                        0);
            }

            var accessToken =
                await GenerateToken(user);

            var refreshToken =
                GenerateRefreshToken();

            user.RefreshToken = refreshToken;

            user.RefreshTokenExpiryTime =
                DateTime.UtcNow.AddDays(7);

            await userManager.UpdateAsync(user);

            int expiry =
                int.Parse(config["Jwt:AccessTokenExpirationMinutes"]!);

            return (
                true,
                "Login Successful",
                accessToken,
                refreshToken,
                expiry
            );
        }

        public async Task<(bool Success, string Message, string UserId)> Register(RegisterDto request)
        {
            if (request.Password != request.ConfirmPassword)
            {
                return (false, "Password Do not Match", string.Empty);
            }
            if (request.Role != "Admin" && request.Role != "Patient" && request.Role != "Doctor")
            {
                return (false, "Invalid Role", string.Empty);
            }

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
            };

            var result = await userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(",", result.Errors.Select(e => e.Description));
                return (false, errors, string.Empty);
            }

            await userManager.AddToRoleAsync(user, request.Role);
            return (true, "User Registered Successfully", user.Id);
        }

        private async Task<string> GenerateToken(ApplicationUser user)
        {
            var jwtSettings = config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var roles = await userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),

                new Claim(JwtRegisteredClaimNames.Sub, user.Id),

                new Claim(JwtRegisteredClaimNames.Email, user.Email!),

                new Claim(ClaimTypes.Email, user.Email!)
            };

            foreach (var r in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, r));
            }

            var expirationMinutes = int.Parse(jwtSettings["AccessTokenExpirationMinutes"]!);

            var token = new JwtSecurityToken(

                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
                signingCredentials: credentials


                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];

            using var rng =
                System.Security.Cryptography.RandomNumberGenerator.Create();

            rng.GetBytes(randomNumber);

            return Convert.ToBase64String(randomNumber);
        }

        public async Task<(bool Success,
                  string Message,
                  string AccessToken,
                  string RefreshToken,
                  int ExpiresIn)>
                    RefreshToken(RefreshTokenDto request)
        {
            var user = userManager.Users
                .FirstOrDefault(u => u.RefreshToken == request.RefreshToken);

            if (user == null)
            {
                return (
                    false,
                    "Invalid Refresh Token",
                    string.Empty,
                    string.Empty,
                    0);
            }

            if (user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return (
                    false,
                    "Refresh Token Expired",
                    string.Empty,
                    string.Empty,
                    0);
            }

            var newAccessToken =
                await GenerateToken(user);

            var newRefreshToken =
                GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;

            user.RefreshTokenExpiryTime =
                DateTime.UtcNow.AddDays(7);

            await userManager.UpdateAsync(user);

            int expiry =
                int.Parse(config["Jwt:AccessTokenExpirationMinutes"]!);

            return (
                true,
                "Token Refreshed Successfully",
                newAccessToken,
                newRefreshToken,
                expiry);
        }
    }
}