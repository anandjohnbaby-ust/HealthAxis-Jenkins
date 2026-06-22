namespace HealthAxis.Shared.DTOs.AuthDtos
{
    public class AuthResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string? Message { get; set; }
        public int ExpiresIn { get; set; }
    }
}
