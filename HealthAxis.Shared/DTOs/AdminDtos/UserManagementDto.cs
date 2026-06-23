namespace HealthAxis.Shared.DTOs.AdminDtos
{
    public class UserManagementDto
    {
        public string UserId { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        public bool EmailConfirmed { get; set; }

        public bool IsActive { get; set; }
    }
}