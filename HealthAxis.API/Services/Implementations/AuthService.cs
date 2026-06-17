using HealthAxis.API.DTOs.AuthDtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HealthAxis.API.Services
{
    public class AuthService(UserManager<IdentityUser> userManager, IConfiguration config) : IAuthService
    {
        public async Task<(bool Success, string Message, string token, int ExpiresIn)> Login(LoginDto request)
        {
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user is null)
            {
                return (false, "Invalid credentials", string.Empty, 0);
            }

            var isPasswordValid = await userManager.CheckPasswordAsync(user, request.Password);
            if (!isPasswordValid)
            {
                return (false, "Invalid credentials", string.Empty, 0);
            }

            var token = await GenerateToken(user);
            var expiry = int.Parse(config.GetSection("Jwt")["AccessTokenExpirationMinutes"]!);
            return (true, "User Logged in Successfully", token, expiry);
        }

        public async Task<(bool Success, string Message, string UserId)> Register(RegisterDto request)
        {
            if (request.Password != request.ConfirmPassword)
            {
                return (false, "Passwords do not match", string.Empty);
            }

            // Allowed roles
            var validRoles = new[] { "Admin", "Patient", "Doctor" };

            // Validate role (case-insensitive)
            if (!validRoles.Any(r =>
                r.Equals(request.Role, StringComparison.OrdinalIgnoreCase)))
            {
                return (false, "Invalid Role. Allowed roles are Admin, Patient, Doctor.", string.Empty);
            }

            // Normalize role value
            request.Role = validRoles.First(r =>
                r.Equals(request.Role, StringComparison.OrdinalIgnoreCase));

            // Check if email already exists
            var existingUser = await userManager.FindByEmailAsync(request.Email);

            if (existingUser != null)
            {
                return (false, "User with this email already exists.", string.Empty);
            }

            var user = new IdentityUser
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

            // Assign role to user
            var roleResult = await userManager.AddToRoleAsync(user, request.Role);

            if (!roleResult.Succeeded)
            {
                var errors = string.Join(", ",
                    roleResult.Errors.Select(e => e.Description));

                // Optional: delete the created user if role assignment fails
                await userManager.DeleteAsync(user);

                return (false, errors, string.Empty);
            }

            return (true,
                $"{request.Role} registered successfully.",
                user.Id);
        }

        private async Task<string> GenerateToken(IdentityUser user)
        {
            var jwtSettings = config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var roles = await userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub,user.Id),
                new Claim(JwtRegisteredClaimNames.Email,user.Email!),
                new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier,user.Id)

            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
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
    }
}