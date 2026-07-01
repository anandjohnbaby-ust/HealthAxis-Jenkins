using HealthAxis.API.Data;
using HealthAxis.API.Exceptions;
using HealthAxis.API.Models;
using HealthAxis.API.Repositories.Implementations;
using HealthAxis.API.Repositories.Interfaces;
using HealthAxis.API.Services.Interfaces;
using HealthAxis.Shared.DTOs.AuthDtos;
using HealthAxis.Shared.DTOs.CommonDtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HealthAxis.API.Services.Implementation
{
    public class AuthService(UserManager<ApplicationUser> userManager, ApplicationDbContext context, IPatientRepository patientRepository, IDoctorRepository doctorRepository, IConfiguration config) : IAuthService
    {
        public async Task<(bool Success, string Message, string AccessToken, string RefreshToken, int ExpiresIn)> Login(LoginDto request)
        {
            var user = await userManager.FindByEmailAsync(request.Email);

            if (user == null)
            {
                return (false, "Invalid Credentials", string.Empty, string.Empty, 0);
            }

            var validPassword = await userManager.CheckPasswordAsync(user, request.Password);

            if (!validPassword)
            {
                return (false, "Invalid Credentials", string.Empty, string.Empty, 0);
            }

            var accessToken = await GenerateToken(user);

            var refreshToken = GenerateRefreshToken();

            user.RefreshToken = refreshToken;

            int refreshTokenExpiryDays = int.Parse(config["Jwt:RefreshTokenExpirationDays"]!);

            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(refreshTokenExpiryDays);

            await userManager.UpdateAsync(user);

            int expiry = int.Parse(config["Jwt:AccessTokenExpirationMinutes"]!);

            return (true, "Login Successful", accessToken, refreshToken, expiry);
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

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var roles = await userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };

            // Add PatientId for patients
            if (roles.Contains("Patient"))
            {
                var patient =
                    await patientRepository.GetByUserIdAsync(user.Id);

                if (patient is not null)
                {
                    claims.Add(
                        new Claim("patientId",
                            patient.PatientId.ToString()));

                    claims.Add(
                        new Claim("fullName",
                            patient.FullName));
                }
            }

            if (roles.Contains("Doctor"))
            {
                var doctor =
                    await doctorRepository.GetByUserIdAsync(user.Id);

                if (doctor is not null)
                {
                    claims.Add(
                        new Claim("doctorId",
                            doctor.DoctorId.ToString()));

                    claims.Add(
                        new Claim("fullName",
                            doctor.FullName));
                }
            }


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

        public async Task ChangePasswordAsync(
            string userId,
            ChangePasswordDto dto)
        {
            var user = await userManager.FindByIdAsync(userId);

            if (user is null)
            {
                throw new NotFoundException("User not found.");
            }

            var result = await userManager.ChangePasswordAsync(
                user,
                dto.CurrentPassword,
                dto.NewPassword);

            if (!result.Succeeded)
            {
                throw new ValidationException(
                    string.Join(
                        Environment.NewLine,
                        result.Errors.Select(e => e.Description)));
            }
        }
    }
}