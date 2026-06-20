using HealthAxis.API.Data;
using HealthAxis.API.DTOs.AuthDtos;
using HealthAxis.API.Models;
using HealthAxis.API.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace HealthAxis.API.Services.Implementation
{
    public class AuthService(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        IConfiguration config) : IAuthService
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
                return (false, "Password and Confirm Password do not match.", string.Empty);
            }

            var existingUser = await userManager.FindByEmailAsync(request.Email);

            if (existingUser != null)
            {
                return (false, "Email already exists.", string.Empty);
            }

            await using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                var user = new ApplicationUser
                {
                    UserName = request.Email,
                    Email = request.Email
                };

                var result = await userManager.CreateAsync(user, request.Password);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ",
                        result.Errors.Select(e => e.Description));

                    return (false, errors, string.Empty);
                }

                await userManager.AddToRoleAsync(user, "Patient");

                var patient = new Patient
                {
                    UserId = user.Id,
                    FullName = request.FullName,
                    DateOfBirth = request.DateOfBirth,
                    Gender = request.Gender,
                    PhoneNumber = request.PhoneNumber,
                    Email = request.Email,
                    CreatedDate = DateTime.UtcNow
                };

                context.Patients.Add(patient);

                await context.SaveChangesAsync();

                await transaction.CommitAsync();

                return
                (
                    true,
                    "Patient registered successfully.",
                    user.Id
                );
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        private async Task<string> GenerateToken(ApplicationUser user)
        {
            var jwtSettings = config.GetSection("Jwt");

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["Key"]!));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var roles = await userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var expirationMinutes =
                int.Parse(jwtSettings["AccessTokenExpirationMinutes"]!);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static string GenerateRefreshToken()
        {
            var bytes = new byte[64];

            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();

            rng.GetBytes(bytes);

            return Convert.ToBase64String(bytes);
        }

        public async Task<(bool Success,
    string Message,
    string AccessToken,
    string RefreshToken,
    int ExpiresIn)> RefreshToken(RefreshTokenDto request)
        {
            var user = await userManager.Users
                .FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken);

            if (user == null)
            {
                return (
                    false,
                    "Invalid Refresh Token.",
                    string.Empty,
                    string.Empty,
                    0);
            }

            if (user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return (
                    false,
                    "Refresh Token has expired.",
                    string.Empty,
                    string.Empty,
                    0);
            }

            var newAccessToken = await GenerateToken(user);

            var newRefreshToken = GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            var result = await userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return (
                    false,
                    "Unable to update refresh token.",
                    string.Empty,
                    string.Empty,
                    0);
            }

            int expiresIn =
                int.Parse(config["Jwt:AccessTokenExpirationMinutes"]!);

            return (
                true,
                "Token refreshed successfully.",
                newAccessToken,
                newRefreshToken,
                expiresIn);
        }
    }
}