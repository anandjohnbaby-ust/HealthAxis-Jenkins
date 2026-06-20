using Microsoft.AspNetCore.Identity;

namespace HealthAxis.API.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Refresh Token
        public string? RefreshToken { get; set; }

        public DateTime RefreshTokenExpiryTime { get; set; }

        // Navigation Properties
        public virtual Patient? Patient { get; set; }

        public virtual Doctor? Doctor { get; set; }
    }
}